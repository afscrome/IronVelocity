
namespace IronVelocity.Parser.AST
{
    public class WordNode : ExpressionNode
    {
        public string Name { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitWordNode(this);
        }
    }
}
