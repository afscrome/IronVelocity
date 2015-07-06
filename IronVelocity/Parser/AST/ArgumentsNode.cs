using System.Collections.Generic;

namespace IronVelocity.Parser.AST
{
    public class ArgumentsNode : SyntaxNode
    {
        public IReadOnlyList<ExpressionNode> Arguments { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitArgumentsNode(this);
        }
    }
}
