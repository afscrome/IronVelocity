using System.Collections.Generic;

namespace IronVelocity.Parser.AST
{
    public class ArgumentsNode : SyntaxNode
    {
        public IReadOnlyList<ExpressionNode> Arguments { get; set; }
    }
}
