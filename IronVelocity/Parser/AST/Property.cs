
namespace IronVelocity.Parser.AST
{
    public class Property : ReferenceNodePart
    {
        public ReferenceNodePart Target { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitProperty(this);
        }
    }
}
