using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        public SyntaxToken LiteralToken { get; }
        public object? Value { get; }

        public LiteralExpressionSyntax(SyntaxToken literalToken, object? value)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public override ImmutableArray<SyntaxNode> GetChildren()
            => ImmutableArray.Create<SyntaxNode>(LiteralToken);
    }
}