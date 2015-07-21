using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class LiteralDirective : CustomDirectiveExpression
    {
        private readonly string _literal;
        public LiteralDirective(ASTDirective node, NVelocityNodeToExpressionConverter converter)
            : base(converter.Builder)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException(nameof(node));

            _literal = node.GetChild(0).Literal;
        }

        protected override Expression ReduceInternal()
        {
            return new RenderedBlock(new[] { Expression.Constant(_literal) });
        }
    }
}
