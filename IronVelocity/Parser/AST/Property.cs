using System;

namespace IronVelocity.Parser.AST
{
    public class Property : ReferenceNodePart
    {
        public ReferenceNodePart Target { get; }

        public Property(string name, ReferenceNodePart target)
            : base(name)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Target = target;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitProperty(this);
        }
    }
}
