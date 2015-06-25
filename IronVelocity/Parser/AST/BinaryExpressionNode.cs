
namespace IronVelocity.Parser.AST
{
    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
        public BinaryOperation Operation { get; set; }
    }
}
