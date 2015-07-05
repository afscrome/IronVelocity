using System.Diagnostics;

namespace IronVelocity.Parser
{
    [DebuggerDisplay("Token: {TokenKind}")]
    public class Token
    {
        public TokenKind TokenKind;
        public string Value;

        public string GetValue()
        {
            switch (TokenKind)
            {
                case TokenKind.Dollar:
                    return "$";
                case TokenKind.Dot:
                    return ".";
                case TokenKind.DotDot:
                    return "..";
                case TokenKind.Exclamation:
                    return "!";
                case TokenKind.EndOfFile:
                    return string.Empty;
                case TokenKind.LeftCurley:
                    return "{";
                case TokenKind.RightCurley:
                    return "}";
                case TokenKind.LeftParenthesis:
                    return "(";
                case TokenKind.RightParenthesis:
                    return ")";
                case TokenKind.LeftSquareBracket:
                    return "[";
                case TokenKind.RightSquareBracket:
                    return "]";
                case TokenKind.Comma:
                    return ",";
                case TokenKind.True:
                    return "true";
                case TokenKind.False:
                    return "false";
                case TokenKind.Plus:
                    return "+";
                case TokenKind.Minus:
                    return "-";
                case TokenKind.Multiply:
                    return "*";
                case TokenKind.Divide:
                    return "/";
                case TokenKind.Modulo:
                    return "%";
                case TokenKind.Identifier:
                case TokenKind.Text:
                case TokenKind.Escaped:
                case TokenKind.Whitespace:
                case TokenKind.NumericLiteral:
                case TokenKind.StringLiteral:
                case TokenKind.InterpolatedStringLiteral:
                case TokenKind.Word:
                default:
                    return Value;
            }
        }
    }
}
