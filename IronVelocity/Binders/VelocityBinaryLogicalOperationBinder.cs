using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace IronVelocity.Binders
{
    public class VelocityBinaryLogicalOperationBinder : DynamicMetaObjectBinder 
    {
        public override Type ReturnType { get { return typeof(bool); } }
        public LogicalOperation Operation { get; private set; }

        public VelocityBinaryLogicalOperationBinder(LogicalOperation type)
        {
            Operation = type;
        }

        public override T BindDelegate<T>(System.Runtime.CompilerServices.CallSite<T> site, object[] args)
        {
            return base.BindDelegate<T>(site, args);
        }
        
        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (args.Length != 1)
                throw new ArgumentOutOfRangeException("args");

            var arg = args[0];
            switch (Operation)
            {
                case LogicalOperation.And:
                case LogicalOperation.Or:
                    return BooleanOperation(target, arg);
                case LogicalOperation.Equal:
                    return Compare(target, arg, Expression.Equal);
                case LogicalOperation.NotEqual:
                    return Compare(target, arg, Expression.NotEqual);
                case LogicalOperation.LessThan:
                    return Compare(target, arg, Expression.LessThan);
                case LogicalOperation.LessThanOrEqual:
                    return Compare(target, arg, Expression.LessThanOrEqual);
                case LogicalOperation.GreaterThan:
                    return Compare(target, arg, Expression.GreaterThan);
                case LogicalOperation.GreaterThanOrEqual:
                    return Compare(target, arg, Expression.GreaterThanOrEqual);
                default:
                    throw new InvalidOperationException("Operation: '" + Operation +"' not supported");
            }


        }



        private DynamicMetaObject BooleanOperation(DynamicMetaObject target, DynamicMetaObject arg)
        {
            if (arg == null)
                throw new ArgumentNullException("arg");

            switch (Operation)
            {
                case LogicalOperation.And:
                case LogicalOperation.Or:
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var left =  CoerceToBoolean(target);
            var right = CoerceToBoolean(arg);

            Expression expression = null;
            BindingRestrictions restrictions = null;
            if (Operation == LogicalOperation.And)
            {
                if (left == null)
                    restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, null);
                else if (right == null)
                    restrictions = BindingRestrictions.GetInstanceRestriction(arg.Expression, null);
                else
                    expression = Expression.AndAlso(left, right);
            }
            else if (Operation == LogicalOperation.Or)
            {
                if (left == null)
                {
                    if (right == null)
                    {
                        expression = null;
                        restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                            .Merge(BindingRestrictions.GetInstanceRestriction(arg.Expression, null));
                    }
                    else
                    {
                        expression = right;
                        restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                            .Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.RuntimeType));
                    }
                }
                else
                {
                    if (right == null)
                    {
                        expression = left;
                        restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
                            .Merge(BindingRestrictions.GetInstanceRestriction(arg.Expression, null));
                    }
                    else
                    {
                        expression = Expression.OrElse(left, right);
                    }
                }
            }
            else
                throw new InvalidOperationException();

            if (expression == null)
                expression = Expression.Constant(false);

            if (restrictions == null)
            {
                restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
                        .Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.RuntimeType));
            }

            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(expression, ReturnType),
                    restrictions
                );
        }


        private static Expression CoerceToBoolean(DynamicMetaObject value)
        {
            if (value.Value == null)
                return null;
            else if (value.LimitType == typeof(bool) || value.LimitType == typeof(bool?))
                return VelocityExpressions.ConvertIfNeeded(value);
            else if (!ReflectionHelper.IsNullableType(value.LimitType))
                return Expression.Constant(true);
            else
                return Expression.NotEqual(value.Expression, Expression.Constant(null, value.Expression.Type));
        }


        private DynamicMetaObject Compare(DynamicMetaObject target, DynamicMetaObject arg, Func<Expression, Expression, Expression> generator)
        {
            //Cannot build a comparison expression until we've resolved the argument type & value
            if (!target.HasValue)
                Defer(target);
            if (!arg.HasValue)
                Defer(arg);


            Expression left, right, mainExpression;
            BinaryOperationHelper.MakeArgumentsCompatible(target, arg, out left, out right);

            try
            {
                mainExpression = generator(left, right);
            }
            catch (InvalidOperationException)
            {
                if (generator == Expression.Equal || generator == Expression.NotEqual)
                {
                    mainExpression = generator(
                        VelocityExpressions.ConvertIfNeeded(left, typeof(object)),
                        VelocityExpressions.ConvertIfNeeded(right, typeof(object))
                    );
                }
                else
                {
                    mainExpression = Expression.Constant(false);
                }
            }

            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    BinaryOperationHelper.DeduceArgumentRestrictions(target).Merge(BinaryOperationHelper.DeduceArgumentRestrictions(arg))
                );

        }




    }

    public enum LogicalOperation
    {
        And,
        Or,

        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual

    }

}
