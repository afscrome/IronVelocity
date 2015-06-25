
namespace IronVelocity.Parser.AST
{
    public class Method : ReferenceNodePart
    {
        public ReferenceNodePart Target { get; set; }
        public ArgumentsNode Arguments { get; set; }
    }
}
