﻿using IronVelocity.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public static class TokenSamples
    {
        public static ImmutableArray<SyntaxKind> LexerTokenKinds { get; } = Enum
            .GetValues(typeof(SyntaxKind))
            .Cast<SyntaxKind>()
            .Where(x => !x.ToString().EndsWith("Expression"))
            .ToImmutableArray();

        public static IReadOnlyCollection<string> GetSamplesForKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.BadToken:
                    return BadToken;
                case SyntaxKind.EndOfFileToken:
                    return EndOfFile;
                case SyntaxKind.SingleLineComment:
                    return SingleLineComment;
                case SyntaxKind.BlockComment:
                    return BlockComment;
                case SyntaxKind.NumberToken:
                    return Number;
                case SyntaxKind.LiteralToken:
                    return Literal;
                case SyntaxKind.HorizontalWhitespaceToken:
                    return HorizontalWhitespace;
                case SyntaxKind.VerticalWhitespaceToken:
                    return VerticalWhitespace;
                case SyntaxKind.DollarToken:
                    return new[] { Dollar };
                case SyntaxKind.HashToken:
                    return new[] { Hash };
                case SyntaxKind.OpenParenthesisToken:
                    return new[] { OpenParenthesis };
                case SyntaxKind.CloseParenthesisToken:
                    return new[] { CloseParenthesis };
                case SyntaxKind.PlusToken:
                    return new[] { Plus };
                case SyntaxKind.MinusToken:
                    return new[] { Minus };
                case SyntaxKind.StarToken:
                    return new[] { Star };
                case SyntaxKind.SlashToken:
                    return new[] { Slash };
                case SyntaxKind.ModuloToken:
                    return new[] { Modulo };
                case SyntaxKind.TrueKeyword:
                    return new[] { True };
                case SyntaxKind.FalseKeyword:
                    return new[] { False };
                case SyntaxKind.IdentifierToken:
                    return Identifier;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, $"No samples defined for SyntaxKind.{kind}");
            }
        }

        public static readonly string Dollar = "$";
        public static readonly string Hash = "#";
        public static readonly string OpenParenthesis = "(";
        public static readonly string CloseParenthesis = ")";
        public static readonly string Plus = "+";
        public static readonly string Minus = "-";
        public static readonly string Star = "*";
        public static readonly string Slash = "/";
        public static readonly string Modulo = "%";
        public static readonly string True = "true";
        public static readonly string False = "false";

        public static readonly IReadOnlyCollection<string> EndOfFile = ImmutableArray<string>.Empty; // End of File is not a real token that can be generated
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
    }
}