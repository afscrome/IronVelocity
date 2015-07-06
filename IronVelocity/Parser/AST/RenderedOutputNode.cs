using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class RenderedOutputNode : SyntaxNode
    {
        public IReadOnlyList<SyntaxNode> Children { get; private set; }

        public RenderedOutputNode(IReadOnlyList<SyntaxNode> children)
        {
            Children = children;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitRenderedOutputNode(this);
        }
    }
}
