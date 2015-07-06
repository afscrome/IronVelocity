
namespace IronVelocity.Parser.AST
{
    public class Variable : ReferenceNodePart
    {
        public Variable(string name)
            : base(name)
        {
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitVariable(this);
        }
    }
}
