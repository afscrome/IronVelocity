using System.Collections.Generic;

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
