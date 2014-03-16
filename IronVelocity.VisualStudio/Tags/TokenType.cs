using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.VisualStudio.Tags
{
    public enum TokenType
    {
        Ignore = 0,
        Literal,
        Operator,
        Keyword,
        Text,
        Comment,
        Identifier,
        StringLiteral,
        NumberLiteral,
        BooleanLiteral,
        Method,
        Block
    }
}
