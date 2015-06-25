using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class ArgumentsNode : SyntaxNode
    {
        public IReadOnlyList<ExpressionNode> Arguments { get; set; }
    }
}
