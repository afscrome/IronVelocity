
namespace IronVelocity.Parser.AST
{
    public class ReferenceNode : ExpressionNode
    {
        public bool IsSilent { get; set; }
        public bool IsFormal { get; set; }
        public ReferenceNodePart Value { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitReferenceNode(this);
        }
    }
}
