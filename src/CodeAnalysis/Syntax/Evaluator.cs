using System;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Evaluator
    {
        private readonly SyntaxTree _syntaxTree;

        public Evaluator(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

        public int Evaluate() => EvaluateExpression(_syntaxTree.Root);

        public int EvaluateExpression(ExpressionSyntax expression)
        {
            switch(expression)
            {
                case UnaryExpressionSyntax u:
                    return EvaluateUnaryExpression(u);

                case LiteralExpressionSyntax n:
                    return (int)n.LiteralToken.Value;

                case ParenthesisedExpressionSyntax p:
                    return EvaluateExpression(p.Expression);

                case BinaryExpressionSyntax b:
                    return EvaluateBinaryExpression(b);

                default:
                    throw new Exception($"Unexpected expression kind {expression.Kind}");

            }
        }

        private int EvaluateUnaryExpression(UnaryExpressionSyntax u)
        {
            switch(u.OperatorToken.Kind)
            {
                case SyntaxKind.PlusToken:
                    return EvaluateExpression(u.Operand);
                case SyntaxKind.MinusToken:
                    return -EvaluateExpression(u.Operand);
                default:
                    throw new ArgumentOutOfRangeException(nameof(u.OperatorToken.Kind), u.OperatorToken.Kind, "Unexpected Unary Operator kind");
            }
        }

        private int EvaluateBinaryExpression(BinaryExpressionSyntax b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            switch (b.OperatorToken.Kind)
            {
                case SyntaxKind.PlusToken:
                    return left + right;
                case SyntaxKind.MinusToken:
                    return left - right;
                case SyntaxKind.StarToken:
                    return left * right;
                case SyntaxKind.SlashToken:
                    return left / right;
                default:
                    throw new Exception($"Unexpected binary operator {b.OperatorToken.Kind}");
            }
        }
    }
}