using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public ExpressionSyntax Expression { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public override ImmutableArray<SyntaxNode> GetChildren()
            => ImmutableArray.Create<SyntaxNode>(Expression);
    }
}