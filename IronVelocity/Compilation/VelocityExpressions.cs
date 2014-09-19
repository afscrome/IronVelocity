using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation
{
    public static class VelocityExpressions
    {
        public static Expression BoxIfNeeded(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return expression.Type.IsValueType && expression.Type != typeof(void)
                ? Expression.Convert(expression, typeof(object))
                : expression;
        }

        public static Expression ConvertIfNeeded(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (type == null)
                throw new ArgumentNullException("type");

            return ConvertIfNeeded(expression, expression.Type, type);
        }

        public static Expression ConvertIfNeeded(DynamicMetaObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return ConvertIfNeeded(obj.Expression, obj.LimitType ?? typeof(object));
        }

        public static Expression ConvertIfNeeded(DynamicMetaObject obj, Type to)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return ConvertIfNeeded(obj.Expression, obj.LimitType, to);
        }

        private static Expression ConvertIfNeeded(Expression expression, Type from, Type to)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            
            if (to == null)
                throw new ArgumentNullException("to");
            
            if (from == null)
                throw new ArgumentNullException("from");

            while (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpression)expression).Operand;

            if (to.IsValueType && !from.IsValueType && (from.IsInterface || from == typeof(object)))
                return Expression.Unbox(expression, to);
            if (expression.Type != from)
                expression = Expression.Convert(expression, from);
            if (from != to || expression.Type != to)
                expression = Expression.Convert(expression, to);

            return expression;
        }


        public static Expression CoerceToBoolean(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (expression.Type == typeof(bool) || expression.Type == typeof(bool?))
                return expression;

            return new AST.CoerceToBooleanExpression(expression);
        }


        public static Expression ConvertParameterIfNeeded(DynamicMetaObject target, ParameterInfo info)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            if (info == null)
                throw new ArgumentNullException("info");

            var expr = target.Expression;
            return ConvertIfNeeded(expr, target.LimitType, info.ParameterType);
        }



    }
}
