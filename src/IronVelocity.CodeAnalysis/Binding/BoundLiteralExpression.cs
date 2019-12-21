using System;

namespace IronVelocity.CodeAnalysis.Binding
{
    public class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object? value)
        {
            Value = value;
        }

        public object? Value { get; }

        public override Type Type => Value?.GetType() ?? typeof(object);

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    }
}
