using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class CoerceToBooleanExpression : VelocityExpression
    {
        public Expression Value { get; private set; }
        public override Type Type { get { return typeof(bool); } }

        public CoerceToBooleanExpression(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Value = expression;
        }

        public override Expression Reduce()
        {
            var expression = Value;

            if (expression.Type == typeof(bool) || expression.Type == typeof(bool?))
                return expression;

            else if (expression.Type.IsValueType)
                return Expression.Constant(true);

            expression = VelocityExpressions.ConvertIfNeeded(expression, typeof(object));

            return Expression.Convert(expression, typeof(bool), MethodHelpers.BooleanCoercionMethodInfo);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            var value = visitor.Visit(Value);

            if (value.Type == typeof(bool))
                return value;

            if (value != Value)
                return new CoerceToBooleanExpression(value);
            else
                return this;
        }


    }
}
