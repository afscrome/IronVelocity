using IronVelocity.CodeAnalysis.Binding;
using System;

namespace IronVelocity.CodeAnalysis
{
    public class Evaluator
    {
        private readonly BoundExpression _root;

        public Evaluator(BoundExpression root)
        {
            _root = root;
        }

        public int Evaluate() => EvaluateExpression(_root);

        public int EvaluateExpression(BoundExpression expression)
        {
            switch (expression)
            {
                case BoundUnaryExpression u:
                    return EvaluateUnaryExpression(u);

                case BoundLiteralExpression n:
                    return (int)n.Value;

                case BoundBinaryExpression b:
                    return EvaluateBinaryExpression(b);

                default:
                    throw new Exception($"Unexpected expression kind {expression.Kind}");

            }
        }

        private int EvaluateUnaryExpression(BoundUnaryExpression expression)
        {
            switch (expression.OperatorKind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return EvaluateExpression(expression.Operand);
                case BoundUnaryOperatorKind.Subtraction:
                    return -EvaluateExpression(expression.Operand);
                default:
                    throw new ArgumentOutOfRangeException(nameof(expression.OperatorKind), expression.OperatorKind, "Unexpected Unary Operator kind");
            }
        }

        private int EvaluateBinaryExpression(BoundBinaryExpression expression)
        {
            var left = EvaluateExpression(expression.Left);
            var right = EvaluateExpression(expression.Right);

            switch (expression.OperatorKind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return left + right;
                case BoundBinaryOperatorKind.Subtraction:
                    return left - right;
                case BoundBinaryOperatorKind.Multiplication:
                    return left * right;
                case BoundBinaryOperatorKind.Division:
                    return left / right;
                default:
                    throw new Exception($"Unexpected binary operator {expression.OperatorKind}");
            }
        }
    }
}