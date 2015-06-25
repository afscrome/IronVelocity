using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class ListExpressionNode : ExpressionNode
    {
        public ListExpressionNode(ExpressionNode singleValue)
        {
            Values = new[] { singleValue };
        }

        public ListExpressionNode(IReadOnlyList<ExpressionNode> values)
        {
            Values = values;
        }

        public IReadOnlyList<ExpressionNode> Values { get; private set; }
    }
}
