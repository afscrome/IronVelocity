using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class TokenSamples
    {
        public static ImmutableArray<SyntaxKind> LexerTokenKinds { get; } = Enum
            .GetValues(typeof(SyntaxKind))
            .Cast<SyntaxKind>()
            .Where(x => !x.ToString().EndsWith("Expression"))
            .ToImmutableArray();

        public static IReadOnlyCollection<string> GetSamplesForKind(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);

            if (text != null)
                return new[] { text };

            return kind switch
            {
                SyntaxKind.BadToken => BadToken,
                SyntaxKind.EndOfFileToken => EndOfFile,
                SyntaxKind.SingleLineComment => SingleLineComment,
                SyntaxKind.BlockComment => BlockComment,
                SyntaxKind.NumberToken => Number,
                SyntaxKind.LiteralToken => Literal,
                SyntaxKind.HorizontalWhitespaceToken => HorizontalWhitespace,
                SyntaxKind.VerticalWhitespaceToken => VerticalWhitespace,
                SyntaxKind.IdentifierToken => Identifier,
                _ => ImmutableArray<string>.Empty
            };
        }

        public static readonly IReadOnlyCollection<string> EndOfFile = ImmutableArray<string>.Empty;
        public static readonly IReadOnlyCollection<string> BadToken = new[] { "?" };

        public static readonly IReadOnlyCollection<string> SingleLineComment = new[]
        {
            "##",
            "##foo"
        };

        public static readonly IReadOnlyCollection<string> BlockComment = new[]
        {
            "#**#",
            "#*foo*#",
            "#* * *#",
            "#* # *#",
            "#*Hello \r \n \r\n World*#"
        };

        public static readonly IReadOnlyCollection<string> Number = new[]
        {
            "0", "1", "2", "3", "4",
            "5", "6", "7", "8", "9",
            "0123456789",
        };

        public static readonly IReadOnlyCollection<string> Identifier = new[]
        {
            "A", "Z", "a", "z", "Identifier"
        };

        public static readonly IReadOnlyCollection<string> Literal = new[]
        {
            "#[[]]",
            "#[[Hello World]]",
            "#[[$]]",
            "#[[#]]",
            @"#[[\]]"
        };

        public static readonly IReadOnlyCollection<string> HorizontalWhitespace = new[]
        {
            " ",
            "\t",
            "   ",
            "\t\t\t",
            " \t \t \t"
        };

        public static readonly IReadOnlyCollection<string> VerticalWhitespace = new[]
        {
            "\r",
            "\n",
            "\r\n",
            "\r\r\r",
            "\n\n\n",
            "\r\n\r\n\r\n",
        };

        [TestCaseSource(nameof(LexerTokenKinds))]
        public void SamplesDefinedForTokenKind(SyntaxKind kind)
        {
            if (kind == SyntaxKind.EndOfFileToken)
            {
                Assert.Pass();
            }

            Assert.That(GetSamplesForKind(kind), Is.Not.Empty);
        }

    }
}
