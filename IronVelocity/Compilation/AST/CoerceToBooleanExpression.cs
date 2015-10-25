using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class CoerceToBooleanExpression : VelocityExpression
    {
        public Expression Value { get; }
        public override Type Type => typeof(bool);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.CoerceToBoolean;

        public CoerceToBooleanExpression(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            Value = expression;
        }

        public override Expression Reduce()
        {
            var expression = Value;

            if (expression.Type == typeof(bool) || expression.Type == typeof(bool?))
                return expression;

            else if (!TypeHelper.IsNullableType(Value.Type))
                return Constants.True;

            expression = VelocityExpressions.ConvertIfNeeded(expression, typeof(object));

            return Expression.Convert(expression, typeof(bool), MethodHelpers.BooleanCoercionMethodInfo);
        }

        public CoerceToBooleanExpression Update(Expression value)
        {
            return value == Value
                ? this
                : new CoerceToBooleanExpression(value);
        }

    }
}
