
namespace IronVelocity.Parser.AST
{
    public class ReferenceNode : ExpressionNode
    {
        public bool IsSilent { get; set; }
        public bool IsFormal { get; set; }
        public ReferenceNodePart Value { get; set; }
    }
}
