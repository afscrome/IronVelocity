using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ReferenceExpression : VelocityExpression
    {
        public string Raw { get; }
        public Expression Value { get; }
        public bool IsSilent { get; }
        public bool IsFormal { get; }

        public ReferenceExpression(Expression value, string raw, bool isSilent, bool isFormal)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentOutOfRangeException(nameof(raw));

            Value = value;
            Raw = raw;
            IsSilent = isSilent;
            IsFormal = IsFormal;
        }

        public override Type Type => Value.Type;
        public override Expression Reduce() => Value;
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.Reference;
    }
}
