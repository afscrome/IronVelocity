using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class VelocityBinaryExpression : VelocityExpression
    {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        protected VelocityBinaryExpression(Expression left, Expression right, SymbolInformation symbols)
        {
            Left = left;
            Right = right;
            Symbols = symbols;
        }

        public abstract VelocityBinaryExpression Update(Expression left, Expression right);

    }
}
