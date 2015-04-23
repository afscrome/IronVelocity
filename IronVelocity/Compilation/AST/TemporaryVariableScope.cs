using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class TemporaryVariableScopeExpression : VelocityExpression
    {
        public ParameterExpression Variable { get; private set; }
        public Expression Body { get; private set; }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.TemporaryVariableScope; } }
        public override Type Type { get { return Body.Type; } }

        public TemporaryVariableScopeExpression(ParameterExpression variable, Expression body)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");

            if (body == null)
                throw new ArgumentNullException("body");

            Variable = variable;
            Body = body;

            var y = Expression.Parameter(typeof(Type), "test");
        }

        public override Expression Reduce()
        {
            return Body;
        }

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
