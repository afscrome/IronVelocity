using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class RenderedOutputNode : SyntaxNode
    {
        public RenderedOutputNode(IReadOnlyList<SyntaxNode> children)
        {
            Children = children;
        }

        public IReadOnlyList<SyntaxNode> Children { get; private set; }
    }
}
