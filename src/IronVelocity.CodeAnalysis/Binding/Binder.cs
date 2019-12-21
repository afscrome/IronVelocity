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
            var op = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);

            if (op == null)
            {
                ReportError($"Binary operator '{syntax.OperatorToken.Text}' not defined for types {left.Type} and {right.Type}");
                return left;
            }

            return new BoundBinaryExpression(left, op, right);
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
            var op = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);

            if (op == null)
            {
                ReportError($"Unary operator '{syntax.OperatorToken.Text}' not defined for type {operand.Type}");
                return operand;
            }


            return new BoundUnaryExpression(op, operand);
        }

        private void ReportError(string message)
        {
            Diagnostics = Diagnostics.Add(message);
        }
    }
}
