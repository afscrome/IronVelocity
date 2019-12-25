using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class SyntaxTree
    {

        public SyntaxTree(IImmutableList<Diagnostic> diagnostics, ExpressionSyntax root)
        {
            Diagnostics = diagnostics;
            Root = root;
        }

        public IImmutableList<Diagnostic> Diagnostics { get; }
        public ExpressionSyntax Root { get; }

    }
}