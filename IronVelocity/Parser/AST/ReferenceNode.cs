using System;

namespace IronVelocity.Parser.AST
{
    public class ReferenceNode : ExpressionNode
    {
        public ReferenceNodePart Value { get; }
        public bool IsSilent { get; }
        public bool IsFormal { get; }

        public ReferenceNode(ReferenceNodePart value, bool isSilent, bool isFormal)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Value = value;
            IsSilent = isSilent;
            IsFormal = isFormal;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitReferenceNode(this);
        }
    }
}
