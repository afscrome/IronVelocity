using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class VelocityComparisonOperationBinder : BinaryOperationBinder 
    {
        private readonly IArgumentConverter _argumentConverter;

        //public override Type ReturnType => typeof(bool);
        public ComparisonOperation Operation { get; }

        public VelocityComparisonOperationBinder(ComparisonOperation type, IArgumentConverter argumentConverter)
            : base(ComparisonOperationToExpressionType(type))
        {
            Operation = type;
            _argumentConverter = argumentConverter;
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            switch (Operation)
            {
                case ComparisonOperation.Equal:
                    return Compare(target, arg, Expression.Equal);
                case ComparisonOperation.NotEqual:
                    return Compare(target, arg, Expression.NotEqual);
                case ComparisonOperation.LessThan:
                    return Compare(target, arg, Expression.LessThan);
                case ComparisonOperation.LessThanOrEqual:
                    return Compare(target, arg, Expression.LessThanOrEqual);
                case ComparisonOperation.GreaterThan:
                    return Compare(target, arg, Expression.GreaterThan);
                case ComparisonOperation.GreaterThanOrEqual:
                    return Compare(target, arg, Expression.GreaterThanOrEqual);
                default:
                    throw new InvalidOperationException("Operation: '" + Operation +"' not supported");
            }
        }


        private DynamicMetaObject Compare(DynamicMetaObject target, DynamicMetaObject arg, Func<Expression, Expression, Expression> generator)
        {
            //Cannot build a comparison expression until we've resolved the argument type & value
            if (!target.HasValue)
                Defer(target);
            if (!arg.HasValue)
                Defer(arg);

            BindingRestrictions restrictions = null;
            Expression  mainExpression = null;

            var left = VelocityExpressions.ConvertIfNeeded(target);
            var right = VelocityExpressions.ConvertIfNeeded(arg);
            _argumentConverter.MakeBinaryOperandsCompatible(target.RuntimeType ?? typeof(object), arg.RuntimeType ?? typeof(object), ref left, ref right);


            bool isEqualityTest = generator == Expression.Equal || generator == Expression.NotEqual;
            if (left.Type == right.Type)
            {
                try
                {
                    mainExpression = generator(left, right);
                }
                catch (InvalidOperationException)
                {
                    bool hasEqualityComponent = generator == Expression.GreaterThanOrEqual || generator == Expression.LessThanOrEqual;
                    if (hasEqualityComponent)
                    {
                        try
                        {
                            mainExpression = Expression.Equal(left, right);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                    else if (isEqualityTest)
                    {
                        mainExpression = generator(VelocityExpressions.ConvertIfNeeded(left, typeof(object)), VelocityExpressions.ConvertIfNeeded(right, typeof(object)));
                    }
                }
            }

            if (mainExpression == null)
            {
                BindingEventSource.Log.ComparisonResolutionFailure(Operation, target.LimitType.FullName, arg.LimitType.FullName);

                mainExpression = generator == Expression.NotEqual
                    ? Constants.True
                    : Constants.False;
            }

            if(restrictions == null)
            {
                restrictions = DeduceArgumentRestrictions(target)
                    .Merge(DeduceArgumentRestrictions(arg));
            }
            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    restrictions
                );
        }

        private static BindingRestrictions DeduceArgumentRestrictions(DynamicMetaObject value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Value != null)
                return BindingRestrictions.GetTypeRestriction(value.Expression, value.RuntimeType);
            else
                return BindingRestrictions.GetInstanceRestriction(value.Expression, null);
        }

        private static ExpressionType ComparisonOperationToExpressionType(ComparisonOperation operation)
        {
            switch (operation)
            {
                case ComparisonOperation.Equal:
                    return ExpressionType.Equal;
                case ComparisonOperation.NotEqual:
                    return ExpressionType.NotEqual;
                case ComparisonOperation.LessThan:
                    return ExpressionType.LessThan;
                case ComparisonOperation.LessThanOrEqual:
                    return ExpressionType.LessThanOrEqual;
                case ComparisonOperation.GreaterThan:
                    return ExpressionType.GreaterThan;
                case ComparisonOperation.GreaterThanOrEqual:
                    return ExpressionType.GreaterThanOrEqual;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, "Invalid enumeration value");
            }
        }
    }

    public enum ComparisonOperation
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual

    }

}
