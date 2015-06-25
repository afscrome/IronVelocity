using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class StringNode : ExpressionNode
    {
        public string Value { get; set; }
        public bool IsInterpolated { get; set; }
    }
}
