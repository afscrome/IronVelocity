using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }
        
        public override ImmutableArray<SyntaxNode> GetChildren()
            => ImmutableArray.Create<SyntaxNode>(Left, OperatorToken, Right);
    }
}