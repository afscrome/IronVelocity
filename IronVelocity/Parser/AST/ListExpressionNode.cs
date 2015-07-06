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

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitListExpressionNode(this);
        }
    }
}
