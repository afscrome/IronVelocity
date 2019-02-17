using IronVelocity.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public static class TokenSamples
    {
        public static IReadOnlyCollection<string> GetSamplesForKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.BadToken:
                    return BadToken;
                case SyntaxKind.EndOfFile:
                    return new[] { EndOfFile };
                case SyntaxKind.SingleLineComment:
                    return SingleLineComment;
                case SyntaxKind.BlockComment:
                    return BlockComment;
                case SyntaxKind.Literal:
                    return Literal;
                case SyntaxKind.HorizontalWhitespace:
                    return HorizontalWhitespace;
                case SyntaxKind.VerticalWhitespace:
                    return VerticalWhitespace;
                case SyntaxKind.Dollar:
                    return new[] { Dollar };
                case SyntaxKind.Hash:
                    return new[] { Hash };
                case SyntaxKind.Plus:
                    return new[] { Plus };
                case SyntaxKind.Minus:
                    return new[] { Minus };
                case SyntaxKind.Star:
                    return new[] { Star };
                case SyntaxKind.Slash:
                    return new[] { Slash };
                case SyntaxKind.Modulo:
                    return new[] { Modulo };
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, $"No samples defined for SyntaxKind.{kind}");
            }
        }

        public static string EndOfFile = "\0";
        public static string Dollar = "$";
        public static string Hash = "#";
        public static string Plus = "+";
        public static string Minus = "-";
        public static string Star = "*";
        public static string Slash = "/";
        public static string Modulo = "%";

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
    }
}
