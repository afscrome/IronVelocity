
namespace IronVelocity.Parser.AST
{
    public class StringNode : ExpressionNode
    {
        public string Value { get; set; }
        public bool IsInterpolated { get; set; }
    }
}
