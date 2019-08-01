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
                case LiteralExpressionSyntax n:
                    return (int)n.LiteralToken.Value;

                case ParenthesisedExpressionSyntax p:
                    return EvaluateExpression(p.Expression);

                case BinaryExpressionSyntax b:
                    var left = EvaluateExpression(b.Left);
                    var right = EvaluateExpression(b.Right);

                    switch(b.OperatorToken.Kind)
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

                default:
                    throw new Exception($"Unexpected expression kind {expression.Kind}");

            }
        }
    }
}