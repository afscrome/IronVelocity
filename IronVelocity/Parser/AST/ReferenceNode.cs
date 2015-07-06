using System;

namespace IronVelocity.Parser.AST
{
    public class ReferenceNode : ExpressionNode
    {
        public ReferenceNodePart Value { get; private set; }
        public bool IsSilent { get; private set; }
        public bool IsFormal { get; private set; }

        public ReferenceNode(ReferenceNodePart value, bool isSilent, bool isFormal)
        {
            if (value == null)
                throw new ArgumentNullException("value");

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
