using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class TemporaryVariableScopeExpression : VelocityExpression
    {
        public new ParameterExpression Variable { get; }
        public Expression Body { get; }
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.TemporaryVariableScope;
        public override Type Type => Body.Type;

        public TemporaryVariableScopeExpression(ParameterExpression variable, Expression body)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            if (body == null)
                throw new ArgumentNullException(nameof(body));

            Variable = variable;
            Body = body;
        }

        public override Expression Reduce() => Body;

        public TemporaryVariableScopeExpression Update(ParameterExpression variable, Expression body)
        {
            return Variable == variable && Body == body
                ? this
                : new TemporaryVariableScopeExpression(variable, body);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return Update(
                Variable,
                visitor.Visit(Body)
                );
        }

    }
}
