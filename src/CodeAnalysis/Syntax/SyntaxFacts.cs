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
                    return 2;

                //Additive
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
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
                    return 3;

                default:
                    return 0;
            }
        }
    }
}
