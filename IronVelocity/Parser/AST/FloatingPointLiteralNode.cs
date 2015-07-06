
namespace IronVelocity.Parser.AST
{
    public class FloatingPointLiteralNode : ExpressionNode
    {
        public float Value { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitFloatingPointLiteralNode(this);
        }
    }
}
