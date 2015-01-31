using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class UnrecognisedDirective : Directive
    {
        private readonly string _literal;
        public override Type Type { get { return typeof(string); } }

        public string Name { get; private set; }
        public UnrecognisedDirective(ASTDirective node, VelocityExpressionBuilder builder)
        {
            _literal = node.Literal;
            Name = node.DirectiveName;
        }

        public override Expression Reduce()
        {
            return Expression.Constant(_literal);
        }


    }
}
