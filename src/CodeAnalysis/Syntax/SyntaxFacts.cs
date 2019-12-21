using IronVelocity.CodeAnalysis.Syntax;
using System;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetBinaryOperatorPrecedence(SyntaxKind kind)
        {
            switch (kind)
            {
                // Multiplicative
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 4;

                //Additive
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 3;

                case SyntaxKind.AmpersandAmpersand:
                    return 2;

                case SyntaxKind.PipePipe:
                    return 1;
                //Not Binary Operator
                default:
                    return 0;
            }
        }


        public static int GetUnaryOperatorPrecedence(SyntaxKind kind)
        {
            switch(kind)
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
            switch(text)
            {
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }
    }
}
