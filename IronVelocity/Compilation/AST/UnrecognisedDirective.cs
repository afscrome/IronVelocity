using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class UnrecognisedDirective : Directive
    {
        private readonly string _literal;
        public override Type Type { get { return typeof(string); } }

        public UnrecognisedDirective(INode node, VelocityExpressionBuilder builder) :base(node, builder)
        {
            _literal = node.Literal;
        }

        public override Expression Reduce()
        {
            return Expression.Constant(_literal);
        }

    }
}
