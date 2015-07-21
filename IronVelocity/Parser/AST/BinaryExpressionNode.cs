using System;

namespace IronVelocity.Parser.AST
{
    public class BinaryExpressionNode : ExpressionNode
    {
        public BinaryOperation Operation { get; }
        public ExpressionNode Left { get; }
        public ExpressionNode Right { get; }

        public BinaryExpressionNode(BinaryOperation operation, ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

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
