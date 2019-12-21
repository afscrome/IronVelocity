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
                case SyntaxKind.AmpersandAmpersand:
                    return 2;

                //Logical Or
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
