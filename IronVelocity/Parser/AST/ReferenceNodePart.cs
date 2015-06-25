using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public abstract class ReferenceNodePart : SyntaxNode
    {
        public string Name { get; set; }
    }
}
