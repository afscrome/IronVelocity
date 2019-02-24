using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract ImmutableArray<SyntaxNode> GetChildren();

        public override string ToString() => ParseTreePrinter.PrettyPrint(this);
    }    
}