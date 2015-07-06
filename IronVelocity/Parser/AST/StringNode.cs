
namespace IronVelocity.Parser.AST
{
    public class StringNode : ExpressionNode
    {
        public string Value { get; set; }
        public bool IsInterpolated { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitStringNode(this);
        }
    }
}
