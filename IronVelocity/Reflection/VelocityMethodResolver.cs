using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class MethodResolver : IMethodResolver
    {

        private readonly IArgumentConverter _conversionHelper;
        public MethodResolver(IArgumentConverter conversionHelper)
        {
            _conversionHelper = conversionHelper;
        }

        public Expression ConvertMethodParameters(MethodInfo method, Expression target, DynamicMetaObject[] args)//, Type[] argTypeArray)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var parameters = method.GetParameters();
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
                    && _conversionHelper.CanBeConverted(argTypeArray.Last(), lastParameter.ParameterType)
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

            Expression result = Expression.Call(
                VelocityExpressions.ConvertIfNeeded(target, method.DeclaringType),
                method,
                argExpressions
            );

            if (method.ReturnType == typeof(void))
            {
                result = Expression.Block(
                    result,
                    Constants.VoidReturnValue
                );
            }

            return result;
        }


        public MethodInfo ResolveMethod(TypeInfo type, string name, params Type[] argTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Loosely based on C# resolution algorithm
            // C# 1.0 resolution algorithm at http://msdn.microsoft.com/en-us/library/aa691336(v=vs.71).aspx
            // C# 5.0 algorithm in section 7.5.3 of spec - http://www.microsoft.com/en-gb/download/details.aspx?id=7029

            //Given the set of applicable candidate function members, the best function member in that set is located.
            var candidates = type.GetRuntimeMethods()
                .Where(x => !x.IsStatic && x.IsPublic)
                .Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Where(x => IsMethodApplicable(x, argTypes))
                .ToList();

            //If the set contains only one function member, then that function member is the best function member.
            if (candidates.Count == 1)
                return candidates.First();

            //Otherwise, the best function member is the one function member that is better than all other function
            //members with respect to the given argument list, provided that each function member is compared to
            //all other function members using the rules in §7.5.3.2.
            return GetBestFunctionMember(candidates);
        }

        public bool IsArgumentCompatible(Type runtimeType, ParameterInfo parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (_conversionHelper.CanBeConverted(runtimeType, parameter.ParameterType))
                return true;

            return IsParameterArrayArgument(parameter)
                && _conversionHelper.CanBeConverted(runtimeType, parameter.ParameterType.GetElementType());
        }

        public bool IsMethodApplicable(MethodInfo method, params Type[] argTypes)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (argTypes == null)
                argTypes = new Type[0];

            //Don't support generic method definitions
            if (method.IsGenericMethod)
                return false;

            var args = method.GetParameters();
            //Do we have a param array?
            var lastArg = args.LastOrDefault();
            ParameterInfo paramsArrayInfo = null;
            if (lastArg != null)
            {
                if (IsParameterArrayArgument(lastArg))
                    paramsArrayInfo = lastArg;
            }


            if (paramsArrayInfo == null && args.Length != argTypes.Length)
                return false;
            else if (argTypes.Length < args.Length - 1)
                return false;


            for (int i = 0; i < argTypes.Length; i++)
            {
                var paramToValidateAgainst = i >= args.Length
                    ? paramsArrayInfo
                    : args[i];

                if (!IsArgumentCompatible(argTypes[i], paramToValidateAgainst))
                    return false;
            }

            return true;
        }


        public MethodInfo GetBestFunctionMember(IEnumerable<MethodInfo> applicableFunctionMembers)
        {
            if (applicableFunctionMembers == null)
                throw new ArgumentNullException(nameof(applicableFunctionMembers));

            if (!applicableFunctionMembers.Any())
                return null;

            var best = new List<MethodInfo>();
            foreach (var candidate in applicableFunctionMembers)
            {
                bool lessSpecific = false;
                foreach (var better in best.ToArray())
                {
                    switch (IsBetterFunctionMember(candidate, better))
                    {
                        //If the current candidate is better than the 'better', remove 'better' from
                        case MethodSpecificityComparison.Better:
                            best.Remove(better);
                            break;
                        case MethodSpecificityComparison.Incomparable:
                            break;
                        case MethodSpecificityComparison.Worse:
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
                return best.First();

            //TODO: Implement Tie-break rules

            //If there is not exactly one function member that is better than all other function members, then the
            //function member invocation is ambiguous and a compile-time error occurs.
            throw new AmbiguousMatchException();
        }


        public MethodSpecificityComparison IsBetterFunctionMember(MethodBase left, MethodBase right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            var leftArgs = left.GetParameters();
            var rightArgs = right.GetParameters();

            if (leftArgs.Length > rightArgs.Length)
                return MethodSpecificityComparison.Better;
            else if (rightArgs.Length > leftArgs.Length)
                return MethodSpecificityComparison.Worse;

            bool leftMoreSpecific = false;
            bool rightMoreSpecific = false;

            for (int i = 0; i < leftArgs.Length; i++)
            {
                var leftType = leftArgs[i].ParameterType;
                var rightType = rightArgs[i].ParameterType;
                //If types the same, then neither is more specific
                if (leftType != rightType)
                {
                    leftMoreSpecific |= _conversionHelper.CanBeConverted(leftType, rightType);
                    rightMoreSpecific |= _conversionHelper.CanBeConverted(rightType, leftType);
                }
            }

            if (leftMoreSpecific == rightMoreSpecific)
                return MethodSpecificityComparison.Incomparable;
            else if (leftMoreSpecific)                        
                return MethodSpecificityComparison.Better;
            else if (rightMoreSpecific)
                return MethodSpecificityComparison.Worse;

            //Should be impossible to get here right??
            throw new InvalidProgramException();
        }


        public static bool IsParameterArrayArgument(ParameterInfo parameter)
        {
            return parameter != null
                && parameter.GetCustomAttributes<ParamArrayAttribute>().Any();
        }



    }

}
