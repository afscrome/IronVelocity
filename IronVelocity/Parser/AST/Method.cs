using System;

namespace IronVelocity.Parser.AST
{
    public class Method : ReferenceNodePart
    {
        public ReferenceNodePart Target { get; private set; }
        public ArgumentsNode Arguments { get; private set; }

        public Method(string name, ReferenceNodePart target, ArgumentsNode arguments)
            : base(name)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            if (arguments == null)
                throw new ArgumentNullException("arguments");

            Target = target;
            Arguments = arguments;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitMethod(this);
        }
    }
}
