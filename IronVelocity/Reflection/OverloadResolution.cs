using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public OverloadResolutionData<T> Resolve<T>(IEnumerable<FunctionMemberData<T>> candidates, IImmutableList<Type> args)
            where T : MemberInfo
        {

            //	Given the set of applicable candidate function members, 
            var applicableCandidateFunctionMembers = ImmutableList.CreateBuilder<OverloadResolutionData<T>>();
            foreach (var candidate in candidates)
            {
                var result = IsApplicableFunctionMember(candidate, args);
                if (result != null)
                    applicableCandidateFunctionMembers.Add(result);
            }

            //If the set contains only one function member, then that function member is the best function member
            if (applicableCandidateFunctionMembers.Count == 1)
                return applicableCandidateFunctionMembers[0];

			//Otherwise, the best function member is the one function member that is better than all other function
			//members with respect to the given argument list, provided that each function member is compared to
			//all other function members using the rules in §7.5.3.2.
			return GetBestFunctionMember(applicableCandidateFunctionMembers.ToImmutable(), args);
        }

        public IImmutableList<Expression> CreateParameterExpressions<T>(OverloadResolutionData<T> overload, DynamicMetaObject[] args)
            where T : MemberInfo
        {
            if (overload == null)
                throw new ArgumentNullException(nameof(overload));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var parameters = overload.Parameters;

            bool isInExpandedForm = overload.ApplicableForm == ApplicableForm.Expanded;

            int fixedParams = isInExpandedForm
                ? parameters.Length - 1
                : parameters.Length;


            var argExpressionBuilder = ImmutableArray.CreateBuilder<Expression>(parameters.Length);

            for (int i = 0; i < fixedParams; i++)
            {
                var parameter = parameters[i];
                argExpressionBuilder.Add(VelocityExpressions.ConvertIfNeeded(args[i], parameter));
            }
            if (isInExpandedForm)
            {
                var lastParameter = overload.Parameters.Last();

                var elementType = lastParameter.GetElementType();
                argExpressionBuilder.Add(Expression.NewArrayInit(
                    elementType,
                    args.Skip(fixedParams)
                        .Select(x => VelocityExpressions.ConvertIfNeeded(x, elementType))
                    ));
            }

            return argExpressionBuilder.ToImmutable();
        }

        private OverloadResolutionData<T> GetBestFunctionMember<T>(IImmutableList<OverloadResolutionData<T>> candidates, IImmutableList<Type> args)
            where T : MemberInfo
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            if (candidates.Count == 0)
                return null;

            var best = new List<OverloadResolutionData<T>>();
            foreach (var candidate in candidates)
            {
                bool lessSpecific = false;
                foreach (var other in best.ToArray())
                {
                    switch (IsBetterFunctionMember(args, candidate, other))
                    {
                        case BetterResult.Better:
                            best.Remove(other);
                            break;
                        case BetterResult.Incomparable:
                            break;
                        case BetterResult.Worse:
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
			//function member invocation is ambiguous.
			return null;
        }


        public OverloadResolutionData<T> IsApplicableFunctionMember<T>(FunctionMemberData<T> functionMember, IImmutableList<Type> argumentList)
            where T : MemberInfo
        {
            var parameters = functionMember.Parameters;
            var lastParam = parameters.LastOrDefault();
            var hasParamsArray = functionMember.HasParamsArray;

            int fixedParams = hasParamsArray
                ? parameters.Length - 1
                : parameters.Length;

            if (!hasParamsArray && argumentList.Count != fixedParams)
                return null;
            else if (argumentList.Count < fixedParams)
                return null;

            // Each argument in A corresponds to a parameter in the function member declaration
            for (int i = 0; i < fixedParams; i++)
            {
                if (!_argumentConverter.CanBeConverted(argumentList[i], parameters[i]))
                    return null;
            }

            ApplicableForm applicableForm;
            if (!hasParamsArray)
            {
                applicableForm = ApplicableForm.Normal;
            }
            // For a function member that includes a parameter array, if the function member is applicable by the above rules, it is said to be applicable in its normal form. 
            else if (argumentList.Count == parameters.Length && _argumentConverter.CanBeConverted(argumentList[fixedParams], parameters[fixedParams]))
            {
                applicableForm = ApplicableForm.Normal;

            }
            // If a function member that includes a parameter array is not applicable in its normal form, the function member may instead be applicable in its expanded form
            else
            {
                var paramArrayElementType = lastParam.GetElementType();
                for (int i = fixedParams; i < argumentList.Count; i++)
                {
                    if (!_argumentConverter.CanBeConverted(argumentList[i], paramArrayElementType))
                        return null;
                }

                applicableForm = ApplicableForm.Expanded;
            }

            return new OverloadResolutionData<T>(applicableForm, functionMember);
        }

        public BetterResult IsBetterFunctionMember<T>(IImmutableList<Type> arguments, OverloadResolutionData<T> left, OverloadResolutionData<T> right)
            where T : MemberInfo
        {

            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            // - for each argument, the implicit conversion from EX to QX is not better than the implicit conversion from EX to PX, and
            // - for at least one argument, the conversion from EX to PX is better than the conversion from EX to QX.
            bool leftIsBetter = false;
            bool rightIsBetter = false;

            for (int i = 0; i < arguments.Count; i++)
            {
                var expressionType = arguments[i];
                var leftParamType = left.GetExpandedParameterType(i);
                var rightParamType = right.GetExpandedParameterType(i);

                leftIsBetter |= IsBetterConversionFromExpression(expressionType, leftParamType, rightParamType);
                rightIsBetter |= IsBetterConversionFromExpression(expressionType, rightParamType, leftParamType);
            }

            if (leftIsBetter != rightIsBetter)
            {
                return leftIsBetter
                    ? BetterResult.Better
                    : BetterResult.Worse;
            }

            // Otherwise, if MP is applicable in its normal form and MQ has a params array and is applicable
            // only in its expanded form, then MP is better than MQ.
            if (left.ApplicableForm != right.ApplicableForm)
            {
                return left.ApplicableForm == ApplicableForm.Normal
                    ? BetterResult.Better
                    : BetterResult.Worse;
            }

            // Otherwise, if MP has more declared parameters than MQ, then MP is better than MQ. This can
            // occur if both methods have params arrays and are applicable only in their expanded forms.
            var leftParamCount = left.Parameters.Length;
            var rightParamCount = right.Parameters.Length;
            if (leftParamCount != rightParamCount)
            {
                return leftParamCount > rightParamCount
                    ? BetterResult.Better
                    : BetterResult.Worse;
            }

            return BetterResult.Incomparable;
        }


        public bool IsBetterConversionFromExpression(Type expression, Type candidate, Type other)
        {
            // E has a type S and an identity conversion exists from S to T1 but not from S to T2
            if (expression == candidate && expression != other)
                return true;

            //T1 is a better conversion target than T2 
            return IsBetterConversionTarget(candidate, other);
        }

        public bool IsBetterConversionTarget(Type candidate, Type other)
        {
            // Given two different types T1 and T2, T1 is a better conversion target than T2 if no implicit conversion from T2 to T1 exists, 
            // and at least one of the following holds:
            if (_argumentConverter.CanBeConverted(other, candidate))
                return false;

            // - An implicit conversion from T1 to T2 exists
            if (_argumentConverter.CanBeConverted(candidate, other))
                return true;

            // - T1 is a signed integral type and T2 is an unsigned integral type
            if (TypeHelper.IsSignedInteger(candidate) && TypeHelper.IsUnsignedInteger(other))
                return true;

            return false;
        }



    }
}
