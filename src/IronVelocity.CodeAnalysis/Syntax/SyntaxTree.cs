using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class SyntaxTree
    {

        public SyntaxTree(IImmutableList<string> diagnostics, ExpressionSyntax root)
        {
            Diagnostics = diagnostics;
            Root = root;
        }

        public IImmutableList<string> Diagnostics { get; }
        public ExpressionSyntax Root { get; }

    }
}