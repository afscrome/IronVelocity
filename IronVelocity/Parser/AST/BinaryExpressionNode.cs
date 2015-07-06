using System;

namespace IronVelocity.Parser.AST
{
    public class BinaryExpressionNode : ExpressionNode
    {
        public BinaryOperation Operation { get; private set; }
        public ExpressionNode Left { get; private set; }
        public ExpressionNode Right { get; private set; }

        public BinaryExpressionNode(BinaryOperation operation, ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException("left");

            if (right == null)
                throw new ArgumentNullException("right");

            Operation = operation;
            Left = left;
            Right = right;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpressionNode(this);
        }

        public BinaryExpressionNode Update(ExpressionNode left, ExpressionNode right)
        {
            return left == Left && right == Right
                ? this
                : new BinaryExpressionNode(Operation, left, right);
        }
    }
}
