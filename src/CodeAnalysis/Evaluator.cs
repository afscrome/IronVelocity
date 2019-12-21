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
            var operandValue = EvaluateExpression(expression.Operand);
            switch (expression.Operator.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return (int)operandValue;
                case BoundUnaryOperatorKind.Negation:
                    return -(int)operandValue;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operandValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(expression.Operator.Kind), expression.Operator.Kind, "Unexpected Unary Operator kind");
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression expression)
        {
            var left = EvaluateExpression(expression.Left);
            var right = EvaluateExpression(expression.Right);

            switch (expression.Operator.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return (int)left + (int)right;
                case BoundBinaryOperatorKind.Subtraction:
                    return (int)left - (int)right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int)left * (int)right;
                case BoundBinaryOperatorKind.Division:
                    return (int)left / (int)right;

                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;

                case BoundBinaryOperatorKind.Equality:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.Inequality:
                    return !Equals(left, right);

                default:
                    throw new Exception($"Unexpected binary operator {expression.Operator.Kind}");
            }
        }
    }
}