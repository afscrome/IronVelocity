namespace IronVelocity.Parser
{
    public enum TokenKind
    {
        None = 0,
        Dollar,
        Dot,
        DotDot,
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
        StringLiteral,
        InterpolatedStringLiteral,
        Word,
        True,
        False,
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,
        Hash,
        Equals,
        Newline
    }
}
