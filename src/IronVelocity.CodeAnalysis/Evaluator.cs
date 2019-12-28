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

        public object? Evaluate() => EvaluateExpression(_root);

        public object? EvaluateExpression(BoundExpression expression)
        {
            return expression.Kind switch
            {
                BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression)expression),
                BoundNodeKind.LiteralExpression => ((BoundLiteralExpression)expression).Value,
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression)expression),
                _ => throw new Exception($"Unexpected expression kind {expression.Kind}"),
            };
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression expression)
        {
            var operandValue = EvaluateExpression(expression.Operand);
            return expression.Operator.Kind switch
            {
                BoundUnaryOperatorKind.Identity => (int)operandValue!,
                BoundUnaryOperatorKind.Negation => -(int)operandValue!,
                BoundUnaryOperatorKind.LogicalNegation => !(bool)operandValue!,
                _ => throw new ArgumentOutOfRangeException(nameof(expression.Operator.Kind), expression.Operator.Kind, "Unexpected Unary Operator kind"),
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression expression)
        {
            var left = EvaluateExpression(expression.Left);
            var right = EvaluateExpression(expression.Right);

            return expression.Operator.Kind switch
            {
                BoundBinaryOperatorKind.Addition => (int)left! + (int)right!,
                BoundBinaryOperatorKind.Subtraction => (int)left! - (int)right!,
                BoundBinaryOperatorKind.Multiplication => (int)left! * (int)right!,
                BoundBinaryOperatorKind.Division => (int)left! / (int)right!,
                BoundBinaryOperatorKind.LogicalAnd => (bool)left! && (bool)right!,
                BoundBinaryOperatorKind.LogicalOr => (bool)left! || (bool)right!,
                BoundBinaryOperatorKind.Equality => Equals(left, right),
                BoundBinaryOperatorKind.Inequality => !Equals(left, right),
                _ => throw new Exception($"Unexpected binary operator {expression.Operator.Kind}"),
            };
        }
    }
}