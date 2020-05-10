
namespace IronVelocity.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        //Token
        BadToken,
        EndOfFileToken,
        HorizontalWhitespaceToken,
        VerticalWhitespaceToken,
        LiteralToken,
        NumberToken,

        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        ModuloToken,

        BangToken,
        AmpersandAmpersandToken,
        PipePipeToken,

        EqualsEqualsToken,
        BangEqualsToken,

        OpenParenthesisToken,
        CloseParenthesisToken,

        IdentifierToken,
        HashToken,
        DollarToken,

        SingleLineCommentToken,
        BlockCommentToken,


        //Nodes
        CompilationUnit,


        //Expressions
        LiteralExpression,
        BinaryExpression,
        ParenthesisedExpression,
        UnaryExpression,


        //Keywords
        TrueKeyword,
        FalseKeyword,
    }
}