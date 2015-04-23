using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class RenderableVelocityReference : RenderableExpression
    {
        public ReferenceExpression Reference { get { return (ReferenceExpression)Expression; } }

        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.RenderableReference; } }

        public RenderableVelocityReference(ReferenceExpression reference)
            : base(reference, reference.Metadata)
        {
        }



    }

    public class RenderableExpression : VelocityExpression
    {
        public Expression Expression { get; private set; }
        public ASTReferenceMetadata Metadata {get; private set;}

        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.RenderableExpression; } }

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
                var evaluatedTemp = Expression.Parameter(typeof(object), "tempEvaulatedResult");

                return new TemporaryVariableScopeExpression(
                    evaluatedTemp,
                    Expression.Condition(
                        Expression.NotEqual(Expression.Assign(evaluatedTemp, expression), Expression.Constant(null, evaluatedTemp.Type)),
                        Expression.Call(
                            MethodHelpers.StringConcatMethodInfo,
                            Expression.Convert(Expression.Constant(prefix), typeof(object)),
                            evaluatedTemp
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
    }

}
