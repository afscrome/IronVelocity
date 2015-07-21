
namespace IronVelocity.Parser.AST
{
    public class FloatingPointLiteralNode : ExpressionNode
    {
        public float Value { get; }

        public FloatingPointLiteralNode(float value)
        {
            Value = value;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitFloatingPointLiteralNode(this);
        }

    }
}
