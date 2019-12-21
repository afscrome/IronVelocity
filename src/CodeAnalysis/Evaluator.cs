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

        public object Evaluate() => EvaluateExpression(_root);

        public object EvaluateExpression(BoundExpression expression)
        {
            switch (expression)
            {
                case BoundUnaryExpression u:
                    return EvaluateUnaryExpression(u);

                case BoundLiteralExpression n:
                    return n.Value;

                case BoundBinaryExpression b:
                    return EvaluateBinaryExpression(b);

                default:
                    throw new Exception($"Unexpected expression kind {expression.Kind}");

            }
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression expression)
        {
            var operandValue = (int)EvaluateExpression(expression.Operand);
            switch (expression.OperatorKind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return operandValue;
                case BoundUnaryOperatorKind.Subtraction:
                    return -operandValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(expression.OperatorKind), expression.OperatorKind, "Unexpected Unary Operator kind");
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression expression)
        {
            var left = (int)EvaluateExpression(expression.Left);
            var right = (int)EvaluateExpression(expression.Right);

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