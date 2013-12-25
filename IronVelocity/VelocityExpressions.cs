using System;
using System.Linq.Expressions;

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

    }
}
