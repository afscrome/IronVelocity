using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity
{
    public static class VelocityExpressions
    {
        public static Expression BoxIfNeeded(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return expression.Type.IsValueType
                ? Expression.Convert(expression, typeof(object))
                : expression;
        }

        public static Expression ConvertIfNeeded(Expression expression, Type type)
        {
            return expression.Type == type
                ? expression
                : Expression.Convert(expression, type);
        }

        public static Expression ConvertIfNeeded(DynamicMetaObject target, MemberInfo member)
        {
            var expr = target.Expression;
            if (member.DeclaringType.IsValueType && !expr.Type.IsValueType)
                return Expression.Unbox(expr, member.DeclaringType);

            return ConvertIfNeeded(expr, member.DeclaringType);
        }

    }
}
