using IronVelocity.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract ImmutableArray<SyntaxNode> GetChildren();

        public override string ToString() => ParseTreePrinter.PrettyPrint(this);

        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;

                return TextSpan.FromBounds(first.Start, last.End);
            }
        }
    }    
}