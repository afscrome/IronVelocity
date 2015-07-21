using System;
using System.Collections.Generic;

namespace IronVelocity.Parser.AST
{
    public class ArgumentsNode : SyntaxNode
    {
        public IReadOnlyList<ExpressionNode> Arguments { get; }

        public ArgumentsNode(IReadOnlyList<ExpressionNode> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            Arguments = arguments;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitArgumentsNode(this);
        }

        public ArgumentsNode Update(IReadOnlyList<ExpressionNode> arguments)
        {
            return arguments == Arguments
                ? this
                : new ArgumentsNode(arguments);
        }
    }
}
