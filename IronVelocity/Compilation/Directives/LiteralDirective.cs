using IronVelocity.Compilation.AST;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.Directives
{
    public class LiteralDirective : CustomDirectiveExpression
    {
        private readonly string _literal;
        private readonly VelocityExpressionBuilder _builder;
        public LiteralDirective(ASTDirective node, VelocityExpressionBuilder builder)
            : base(node, builder)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 1)
                throw new ArgumentOutOfRangeException("node");

            _literal = node.GetChild(0).Literal;
            _builder = builder;
        }

        protected override Expression ReduceInternal()
        {
            return new RenderedBlock(new[] { Expression.Constant(_literal) }, _builder);
        }
    }
}
