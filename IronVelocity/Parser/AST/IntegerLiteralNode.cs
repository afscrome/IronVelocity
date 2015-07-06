
namespace IronVelocity.Parser.AST
{
    public class IntegerLiteralNode : ExpressionNode
    {
        public int Value { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitIntegerLiteralNode(this);
        }
    }
}
