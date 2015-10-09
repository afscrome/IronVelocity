using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class UnrecognisedDirective : Directive
    {
        private readonly string _literal;
        public override Type Type { get { return typeof(string); } }

        public string Name { get; }
        public UnrecognisedDirective(ASTDirective node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            _literal = node.Literal;
            Name = node.DirectiveName;
        }

        public UnrecognisedDirective(string name, string literal)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            if (string.IsNullOrWhiteSpace(literal))
                throw new ArgumentOutOfRangeException(nameof(literal));

            _literal = literal;
            Name = name;
        }


        public override Expression Reduce()
        {
            return Expression.Constant(_literal);
        }


    }
}
