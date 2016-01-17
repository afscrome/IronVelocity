using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class OverloadResolver : IOverloadResolver
    {
        private readonly IArgumentConverter _argumentConverter;

        public OverloadResolver(IArgumentConverter argumentConverter)
        {
            if (argumentConverter == null)
                throw new ArgumentNullException(nameof(argumentConverter));

            _argumentConverter = argumentConverter;
        }

        public FunctionMemberData<T> Resolve<T>(IEnumerable<FunctionMemberData<T>> candidates, Type[] args)
        {
            //	Given the set of applicable candidate function members, 
            var aplicableCandidateFunctionMembers = candidates
                .Where(x => IsApplicableFunctionMember(x.Parameters, args))
                .ToList();

            //If the set contains only one function member, then that function member is the best function member
            if (aplicableCandidateFunctionMembers.Count == 1)
                return aplicableCandidateFunctionMembers.First();

            //Otherwise, the best function member is the one function member that is better than all other function
            //members with respect to the given argument list, provided that each function member is compared to
            //all other function members using the rules in §7.5.3.2.
            return GetBestFunctionMember(aplicableCandidateFunctionMembers);
        }

        public Expression[] CreateParameterExpressions(ParameterInfo[] parameters, DynamicMetaObject[] args)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var lastParameter = parameters.LastOrDefault();
            bool hasParamsArray = IsParameterArrayArgument(lastParameter);

            int trivialParams = hasParamsArray
                ? parameters.Length - 1
                : parameters.Length;

            var argTypeArray = args
                .Select(x => x.Value == null ? null : x.LimitType)
                .ToArray();


            var argExpressions = new Expression[parameters.Length];
            for (int i = 0; i < trivialParams; i++)
            {
                var parameter = parameters[i];
                argExpressions[i] = VelocityExpressions.ConvertParameterIfNeeded(args[i], parameter);
            }
            if (hasParamsArray)
            {
                int lastIndex = argExpressions.Length - 1;
                //Check if the array has been explicitly passed, rather than as individual elements
                if (args.Length == parameters.Length
                    && _argumentConverter.CanBeConverted(argTypeArray.Last(), lastParameter.ParameterType)
                    && argTypeArray.Last() != null)
                {
                    argExpressions[lastIndex] = VelocityExpressions.ConvertParameterIfNeeded(args[lastIndex], lastParameter);
                }
                else
                {
                    var elementType = lastParameter.ParameterType.GetElementType();
                    argExpressions[lastIndex] = Expression.NewArrayInit(
                        elementType,
                        args.Skip(lastIndex)
                            .Select(x => VelocityExpressions.ConvertIfNeeded(x, elementType))
                        );
                }
            }

            return argExpressions;
        }

        private FunctionMemberData<T> GetBestFunctionMember<T>(IReadOnlyCollection<FunctionMemberData<T>> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            if (candidates.Count == 0)
                return null;

            var best = new List<FunctionMemberData<T>>();
            foreach (var candidate in candidates)
            {
                bool lessSpecific = false;
                foreach (var better in best.ToArray())
                {
                    switch (IsBetterFunctionMember(candidate.Parameters, better.Parameters))
                    {
                        //If the current candidate is better than the 'better', remove 'better' from
                        case FunctionMemberComparisonResult.Better:
                            best.Remove(better);
                            break;
                        case FunctionMemberComparisonResult.Incomparable:
                            break;
                        case FunctionMemberComparisonResult.Worse:
                            lessSpecific = true;
                            continue;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                if (!lessSpecific)
                {
                    best.Add(candidate);
                }
            }

            //If the set contains only one function member, then that function member is the best function member.
            if (best.Count == 1)
                return best[0];

            //If there is not exactly one function member that is better than all other function members, then the
            //function member invocation is ambiguous and a compile-time error occurs.
            throw new AmbiguousMatchException();
        }


        public bool IsApplicableFunctionMember(ParameterInfo[] parameters, params Type[] argumentList)
        {
            var lastArg = parameters.LastOrDefault();
            ParameterInfo paramsArrayInfo = null;
            if (IsParameterArrayArgument(lastArg))
                    paramsArrayInfo = lastArg;

            //If there is no params array, the argument count must match the parameter count
            //This will not be true if we support default parameters
            if (paramsArrayInfo == null && parameters.Length != argumentList.Length)
                return false;
            //If there is a params parameter, the argument list must not be shorter than the parameter list without the params parameter
            else if (argumentList.Length < parameters.Length - 1)
                return false;

            for (int i = 0; i < argumentList.Length; i++)
            {
                var paramToValidateAgainst = i >= parameters.Length
                    ? paramsArrayInfo
                    : parameters[i];

                if (!IsArgumentCompatible(argumentList[i], paramToValidateAgainst))
                    return false;
            }

            return true;
        }


        public FunctionMemberComparisonResult IsBetterFunctionMember(ParameterInfo[] left, ParameterInfo[] right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));


            if (left.Length > right.Length)
                return FunctionMemberComparisonResult.Better;
            else if (right.Length > left.Length)
                return FunctionMemberComparisonResult.Worse;

            bool leftMoreSpecific = false;
            bool rightMoreSpecific = false;

            for (int i = 0; i < left.Length; i++)
            {
                var leftType = left[i].ParameterType;
                var rightType = right[i].ParameterType;
                //If types the same, then neither is more specific
                if (leftType != rightType)
                {
                    leftMoreSpecific |= _argumentConverter.CanBeConverted(leftType, rightType);
                    rightMoreSpecific |= _argumentConverter.CanBeConverted(rightType, leftType);
                }
            }

            //TODO: Implement Tie-break rules

            if (leftMoreSpecific == rightMoreSpecific)
                return FunctionMemberComparisonResult.Incomparable;
            else if (leftMoreSpecific)
                return FunctionMemberComparisonResult.Better;
            else if (rightMoreSpecific)
                return FunctionMemberComparisonResult.Worse;

            //Should be impossible to get here right??
            throw new InvalidProgramException();
        }


        public bool IsArgumentCompatible(Type runtimeType, ParameterInfo parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (_argumentConverter.CanBeConverted(runtimeType, parameter.ParameterType))
                return true;

            return IsParameterArrayArgument(parameter)
                && _argumentConverter.CanBeConverted(runtimeType, parameter.ParameterType.GetElementType());
        }


        public bool IsParameterArrayArgument(ParameterInfo parameter)
        {
            return parameter != null
                && parameter.GetCustomAttributes<ParamArrayAttribute>().Any();
        }

    }
}
