using System;

namespace IronVelocity.Parser.AST
{
    public class Method : ReferenceNodePart
    {
        public ReferenceNodePart Target { get; }
        public ArgumentsNode Arguments { get; }

        public Method(string name, ReferenceNodePart target, ArgumentsNode arguments)
            : base(name)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            Target = target;
            Arguments = arguments;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitMethod(this);
        }
    }
}
