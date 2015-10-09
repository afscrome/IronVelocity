using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class RenderedBlock : VelocityExpression
    {
        public override Type Type => typeof(void);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.RenderedBlock;

        public RenderedBlock(IEnumerable<Expression> expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            Children = expressions.ToList();
        }


        public IReadOnlyCollection<Expression> Children { get; }

        public override Expression Reduce()
        {
            if (!Children.Any())
                return Constants.EmptyExpression;

            var convertedExpressions = Children
                .Select(Output);

            return Expression.Block(typeof(void), convertedExpressions);
        }

        private Expression Output(Expression expression)
        {
            if (expression.Type == typeof(void))
            {
                return expression;
            }

            var reference = expression as ReferenceExpression;
            if (reference != null)
            {
                if (!reference.IsSilent)
                    return new RenderableExpression(reference, reference.Raw);
            }

            var method = expression.Type == typeof(string)
                ? MethodHelpers.OutputStringMethodInfo
                : MethodHelpers.OutputObjectMethodInfo;

            return Expression.Call(Constants.OutputParameter, method, expression);
        }



    }
}
