using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class DirectiveNode : SyntaxNode
    {
        public string Name { get; }

        public DirectiveNode(string name)
        {
            Name = name;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitDirective(this);
        }
    }
}
