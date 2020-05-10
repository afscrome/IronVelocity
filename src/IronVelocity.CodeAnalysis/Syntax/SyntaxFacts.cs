using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        private static readonly ImmutableArray<SyntaxKind> _kinds = Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().ToImmutableArray();

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds() => _kinds.Where(x => GetUnaryOperatorPrecedence(x) > 0);
        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds() => _kinds.Where(x => GetBinaryOperatorPrecedence(x) > 0);

        public static int GetBinaryOperatorPrecedence(SyntaxKind kind)
        {
            switch (kind)
            {
                // Multiplicative
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 5;

                //Additive
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4;

                //Equality
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.BangEqualsToken:
                    return 3;

                //Logical And
                case SyntaxKind.AmpersandAmpersandToken:
                    return 2;

                //Logical Or
                case SyntaxKind.PipePipeToken:
                    return 1;

                //Not Binary Operator
                default:
                    return 0;
            }
        }


        public static int GetUnaryOperatorPrecedence(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:
                    return 1;

                default:
                    return 0;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static string? GetText(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.DollarToken => "$",
                SyntaxKind.HashToken => "#",
                SyntaxKind.OpenParenthesisToken => "(",
                SyntaxKind.CloseParenthesisToken => ")",
                SyntaxKind.PlusToken => "+",
                SyntaxKind.MinusToken => "-",
                SyntaxKind.StarToken => "*",
                SyntaxKind.SlashToken => "/",
                SyntaxKind.ModuloToken => "%",
                SyntaxKind.TrueKeyword => "true",
                SyntaxKind.FalseKeyword => "false",
                SyntaxKind.BangToken => "!",
                SyntaxKind.BangEqualsToken => "!=",
                SyntaxKind.EqualsEqualsToken => "==",
                SyntaxKind.AmpersandAmpersandToken => "&&",
                SyntaxKind.PipePipeToken => "||",
                _ => null
            };
        }
    }
}
