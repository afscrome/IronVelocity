using IronVelocity.Compilation;
using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class VelocityComparisonOperationBinder : DynamicMetaObjectBinder 
    {
        public override Type ReturnType { get { return typeof(bool); } }
        public ComparisonOperation Operation { get; }

        public VelocityComparisonOperationBinder(ComparisonOperation type)
        {
            Operation = type;
        }
        
        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (args == null)
                throw new ArgumentNullException(nameof(args));


            if (args.Length != 1)
                throw new ArgumentOutOfRangeException(nameof(args));

            var arg = args[0];
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

            Expression left, right;
            BinaryOperationHelper.MakeArgumentsCompatible(target, arg, out left, out right);


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
                restrictions = BinaryOperationHelper.DeduceArgumentRestrictions(target)
                    .Merge(BinaryOperationHelper.DeduceArgumentRestrictions(arg));
            }
            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    restrictions
                );
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
