namespace IronVelocity.Parser
{
    public enum TokenKind
    {
        Dollar,
        Dot,
        Exclamation,
        EndOfFile,
        Identifier,
        LeftCurley,
        RightCurley,
        LeftParenthesis,
        RightParenthesis,
        LeftSquareBracket,
        RightSquareBracket,
        Text,
        Escaped,
        Whitespace,
        Comma,
        NumericLiteral,
        Dash,
        StringLiteral,
        InterpolatedStringLiteral,
        Word,
        True,
        False
    }
}
