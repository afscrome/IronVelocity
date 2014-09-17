using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class RenderableVelocityReference : RenderableExpression
    {
        public ReferenceExpression Reference { get { return (ReferenceExpression)Expression; } }

        public RenderableVelocityReference(ReferenceExpression reference)
            : base(reference, reference.Metadata)
        {
        }



    }

    public class RenderableExpression : VelocityExpression
    {
        public Expression Expression { get; private set; }
        public ASTReferenceMetadata Metadata {get; private set;}

        public RenderableExpression(Expression expression, ASTReferenceMetadata metadata)
        {
            Expression = expression;
            Metadata = metadata;
        }

        public override Expression Reduce()
        {
            if (Metadata.Escaped)
            {
                return Expression.Condition(
                    Expression.NotEqual(Expression, Expression.Constant(null, Expression.Type)),
                    Expression.Constant(Metadata.EscapePrefix + Metadata.NullString),
                    Expression.Constant(Metadata.EscapePrefix + "\\" + Metadata.NullString)
                );
            }
            else
            {
                var prefix = Metadata.EscapePrefix + Metadata.MoreString;
                var NullValue = Expression.Constant(Metadata.EscapePrefix + prefix + Metadata.NullString);

                //TODO: work a way around this if possible
                //For static typing

                var expression = Expression;
                if (!Expression.Type.IsAssignableFrom(typeof(string)))
                {
                    expression = VelocityExpressions.ConvertIfNeeded(expression, typeof(object));
                }

                //If the literal has not been escaped (has an empty prefix), then we can return a simple Coalesce expression
                if (String.IsNullOrEmpty(prefix))
                    return expression.Type.IsValueType
                        ? expression
                        : Expression.Coalesce(expression, NullValue);

                //Otherwise we have to do a slightly more complicated result
                var _evaulatedResult = Expression.Parameter(typeof(object), "tempEvaulatedResult");
                return Expression.Block(
                    new[] { _evaulatedResult },
                    Expression.Assign(_evaulatedResult, expression),
                    Expression.Condition(
                        Expression.NotEqual(_evaulatedResult, Expression.Constant(null, _evaulatedResult.Type)),
                        Expression.Call(
                            MethodHelpers.StringConcatMethodInfo,
                            Expression.Convert(Expression.Constant(prefix), typeof(object)),
                            _evaulatedResult
                        ),
                        NullValue
                    )
                );
            }
        }

        public RenderableExpression Update(Expression expression)
        {
            return Expression == expression
                ? this
                : new RenderableExpression(expression, Metadata);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            var expression = visitor.Visit(Expression);
            return this.Update(expression);
        }
    }

}
