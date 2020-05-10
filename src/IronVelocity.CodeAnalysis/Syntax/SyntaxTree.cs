using IronVelocity.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class SyntaxTree
    {

        public SyntaxTree(SourceText text)
        {
            var parser = new Parser(text);
            var root = parser.ParseCompilationUnit();

            Diagnostics = parser.Diagnostics.ToImmutableArray();
            Root = root;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public CompilationUnitSyntax Root { get; }


        public static SyntaxTree Parse(string text)
            => new SyntaxTree(new SourceText(text));
        public static SyntaxTree Parse(SourceText text)
            => new SyntaxTree(text);


        public static ImmutableArray<SyntaxToken> ParseTokens(string text)
            => ParseTokens(new SourceText(text));

        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
        {
            var lexer = new Lexer(text);
            var builder = ImmutableArray.CreateBuilder<SyntaxToken>();

            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                builder.Add(token);
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return builder.ToImmutable();
        }

    }
}