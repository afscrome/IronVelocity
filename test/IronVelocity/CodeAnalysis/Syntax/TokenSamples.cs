﻿using IronVelocity.CodeAnalysis.Syntax;
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
                    return EndOfFile;
                case SyntaxKind.SingleLineComment:
                    return SingleLineComment;
                case SyntaxKind.BlockComment:
                    return BlockComment;
                case SyntaxKind.Literal:
                    return Literal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, $"No samples defined for SyntaxKind.{kind}");
            }
        }


        public static readonly IReadOnlyCollection<string> BadToken = new[] { "?" };
        public static readonly IReadOnlyCollection<string> EndOfFile = new[] { "\0" };

        public static readonly IReadOnlyCollection<string> SingleLineComment = new[] {
            "##",
            "##foo"
        };

        public static readonly IReadOnlyCollection<string> BlockComment = new[] {
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
}
}
