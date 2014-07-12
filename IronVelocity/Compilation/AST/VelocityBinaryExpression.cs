using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract class VelocityBinaryExpression : VelocityExpression
    {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        protected VelocityBinaryExpression(INode node)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.ChildrenCount != 2)
                throw new NotImplementedException("Expected exactly two children for a binary expression");


            Left = VelocityExpressionBuilder.Operand(node.GetChild(0));
            Right = VelocityExpressionBuilder.Operand(node.GetChild(1));

        }

        protected VelocityBinaryExpression(Expression left, Expression right, SymbolInformation symbols)
        {
            Left = left;
            Right = right;
            Symbols = symbols;
        }

        public abstract Expression Update(Expression left, Expression right);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            var left = visitor.Visit(Left);
            var right = visitor.Visit(Right);

            return visitor.Visit(Update(left, right).ReduceAndCheck());
        }


    }
}
