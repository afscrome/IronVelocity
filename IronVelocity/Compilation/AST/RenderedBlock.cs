using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class RenderedBlock : VelocityExpression
    {
        private readonly ParameterExpression _output;

        public RenderedBlock(INode node, VelocityExpressionBuilder builder)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (builder == null)
                throw new ArgumentNullException("builder");

            //ASTprocess is a special case for the root, otherwise it behaves exactly like ASTBlock
            if (!(node is ASTBlock || node is ASTprocess))
                throw new ArgumentOutOfRangeException("node");

            Children = builder.GetBlockExpressions(node);

            _output = builder.OutputParameter;
        }

        public RenderedBlock(IEnumerable<Expression> expressions, VelocityExpressionBuilder builder)
        {
            if (expressions == null)
                throw new ArgumentNullException("expressions");

            if (builder == null)
                throw new ArgumentNullException("builder");

            Children = expressions.ToList();
            _output = builder.OutputParameter;
        }


        public IReadOnlyCollection<Expression> Children { get; private set; }

        public override Expression Reduce()
        {
            if (!Children.Any())
                return Expression.Default(typeof(void));

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

            var reference = expression as DynamicReference;
            if (reference != null)
                expression = new RenderableDynamicReference(reference);


            if (expression.Type != typeof(string))
                expression = Expression.Call(expression, MethodHelpers.ToStringMethodInfo);

            return Expression.Call(_output, MethodHelpers.AppendMethodInfo, expression);
        }



        public override Type Type { get { return typeof(void); } }
    }
}
