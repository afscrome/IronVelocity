using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class TextNode : SyntaxNode
    {
        public string Content { get; private set; }
        public TextNode(string content)
        {
            Content = content;
        }
    }
}
