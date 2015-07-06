
namespace IronVelocity.Parser.AST
{
    public class Method : ReferenceNodePart
    {
        public ReferenceNodePart Target { get; set; }
        public ArgumentsNode Arguments { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitMethod(this);
        }
    }
}
