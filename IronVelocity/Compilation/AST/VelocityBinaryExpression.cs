using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class VelocityBinaryExpression : VelocityExpression
    {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        protected VelocityBinaryExpression(Expression left, Expression right, SourceInfo sourceInfo)
        {
            Left = left;
            Right = right;
            SourceInfo = sourceInfo;
        }

        public abstract VelocityBinaryExpression Update(Expression left, Expression right);

    }
}
