using IronVelocity.CodeAnalysis.Syntax;
using System;
using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Binding
{
    public class Binder
    {
        public IImmutableList<string> Diagnostics { get; private set; } = ImmutableList<string>.Empty;

        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.ParenthesisedExpression:
                    return BindParenthesisedExpression((ParenthesisedExpressionSyntax)syntax);
                default:
                    throw new ArgumentOutOfRangeException(nameof(syntax.Kind), syntax.Kind, "Unexpected syntax kind");
            }
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var right = BindExpression(syntax.Right);
            var operatorKind = BindBinaryOperatorKind(syntax.OperatorToken.Kind, left.Type, right.Type);

            if (operatorKind == null)
            {
                ReportError($"Binary operator '{syntax.OperatorToken.Text}' not defined for types {left.Type} and {right.Type}");
                return left;
            }

            return new BoundBinaryExpression(left, operatorKind.Value, right);
        }


        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            return new BoundLiteralExpression(syntax.Value);
        }

        private BoundExpression BindParenthesisedExpression(ParenthesisedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var operand = BindExpression(syntax.Operand);
            var operatorKind = BindUnaryOperatorKind(syntax.OperatorToken.Kind, operand.Type);

            if (operatorKind == null)
            {
                ReportError($"Unary operator '{syntax.OperatorToken.Text}' not defined for type {operand.Type}");
                return operand;
            }


            return new BoundUnaryExpression(operatorKind.Value, operand);
        }

        private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
        {
            if (leftType != typeof(int) || rightType != typeof(int))
            {
                return null;
            }

            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return BoundBinaryOperatorKind.Addition;
                case SyntaxKind.MinusToken:
                    return BoundBinaryOperatorKind.Subtraction;
                case SyntaxKind.StarToken:
                    return BoundBinaryOperatorKind.Multiplication;
                case SyntaxKind.SlashToken:
                    return BoundBinaryOperatorKind.Division;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unexpected Binary Operator");
            }
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if (operandType != typeof(int))
            {
                return null;
            }

            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return BoundUnaryOperatorKind.Identity;
                case SyntaxKind.MinusToken:
                    return BoundUnaryOperatorKind.Subtraction;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unexpected Unary Operator");
            }
        }

        private void ReportError(string message)
        {
            Diagnostics = Diagnostics.Add(message);
        }
    }
}
