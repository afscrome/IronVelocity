using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class VelocityBinaryExpression : VelocityExpression
    {
        public Expression Left { get; }
        public Expression Right { get; }

        protected VelocityBinaryExpression(Expression left, Expression right, SourceInfo sourceInfo)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            Left = left;
            Right = right;
            SourceInfo = sourceInfo;
        }

        public abstract VelocityBinaryExpression Update(Expression left, Expression right);

    }
}
