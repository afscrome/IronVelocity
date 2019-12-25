using IronVelocity.CodeAnalysis.Syntax;
using System;
using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Binding
{
    public class Binder
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

        public IImmutableList<Diagnostic> Diagnostics => _diagnostics.Diagnostics;

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
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken, left.Type, right.Type);
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
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken, operand.Type);
                return operand;
            }


            return new BoundUnaryExpression(op, operand);
        }
    }
}
