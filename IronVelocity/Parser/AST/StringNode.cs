using System;

namespace IronVelocity.Parser.AST
{
    public class StringNode : ExpressionNode
    {
        public string Value { get; }
        public bool IsInterpolated { get; }

        public StringNode(string value, bool isInterpolated)
        {
            if (value == null) //Empty string & whitespace is allowed, so only check against null
                throw new ArgumentNullException(nameof(value));

            Value = value;
            IsInterpolated = isInterpolated;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitStringNode(this);
        }
    }
}
