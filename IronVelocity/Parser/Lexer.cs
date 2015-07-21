using System;
using System.Text;

namespace IronVelocity.Parser
{
    public enum LexerState
    {
        Text,
        Vtl
    }

    public class Lexer
    {
        private readonly ITextWindow _input;

        private StringBuilder _builder = new StringBuilder();
        private char _nextChar;
        public LexerState State {get; set;}

        public Lexer(string input, LexerState state)
        {
            _input = new StringTextWindow(input);
            State = state;
            _nextChar = _input.CurrentChar;
        }


        public Token GetNextToken()
        {
            switch (State)
            {
                case LexerState.Text:
                    return GetNextTextToken();
                case LexerState.Vtl:
                   return GetNextVtlToken();
                default:
                    throw new ArgumentOutOfRangeException(nameof(State));
            }
        }


        public Token GetNextTextToken()
        {
            var token = new Token();
            switch (_nextChar)
            {
                case '\0':
                    token.TokenKind = TokenKind.EndOfFile;
                    break;
                case '#':
                    token.TokenKind = TokenKind.Hash;
                    Advance();
                    break;
                case '$':
                    token.TokenKind = TokenKind.Dollar;
                    Advance();
                    break;
                default:
                    Text(ref token);
                    break;
            }

            return token;
        }

        public void Text(ref Token token)
        {
            int startPosition = _input.CurrentPosition;
            char nextChar = _nextChar;

            _builder.Clear();
            while (true)
            {
                switch (nextChar)
                {
                    case '\0':
                    case '$':
                    //case '#':
                        token.TokenKind = TokenKind.Text;
                        token.Value = _builder.ToString();
                        return;
                    default:
                        _builder.Append(nextChar);
                        nextChar = Advance();
                        break;

                }
            }

        }

        public Token GetNextVtlToken()
        {
            var token = new Token();

            char currentChar = _nextChar;
            switch (currentChar)
            {
                case '\0':
                    token.TokenKind = TokenKind.EndOfFile;
                    break;
                case '$':
                    token.TokenKind = TokenKind.Dollar;
                    Advance();
                    break;
                case '#':
                    token.TokenKind = TokenKind.Hash;
                    Advance();
                    break;
                case '!':
                    token.TokenKind = TokenKind.Exclamation;
                    Advance();
                    break;
                case '{':
                    token.TokenKind = TokenKind.LeftCurley;
                    Advance();
                    break;
                case '}':
                    token.TokenKind = TokenKind.RightCurley;
                    Advance();
                    break;
                case '.':
                    currentChar = Advance();
                    if (currentChar == '.')
                    {
                        token.TokenKind = TokenKind.DotDot;
                        Advance();
                    }
                    else
                    {
                        token.TokenKind = TokenKind.Dot;
                    }


                    break;
                case '(':
                    token.TokenKind = TokenKind.LeftParenthesis;
                    Advance();
                    break;
                case ')':
                    token.TokenKind = TokenKind.RightParenthesis;
                    Advance();
                    break;
                case '[':
                    token.TokenKind = TokenKind.LeftSquareBracket;
                    Advance();
                    break;
                case ']':
                    token.TokenKind = TokenKind.RightSquareBracket;
                    Advance();
                    break;
                case ',':
                    token.TokenKind = TokenKind.Comma;
                    Advance();
                    break;
                case '-':
                    token.TokenKind = TokenKind.Minus;
                    Advance();
                    break;
                case '+':
                    token.TokenKind = TokenKind.Plus;
                    Advance();
                    break;
                case '*':
                    token.TokenKind = TokenKind.Multiply;
                    Advance();
                    break;
                case '/':
                    token.TokenKind = TokenKind.Divide;
                    Advance();
                    break;
                case '%':
                    token.TokenKind = TokenKind.Modulo;
                    Advance();
                    break;
                case '=':
                    token.TokenKind = TokenKind.Equals;
                    Advance();
                    break;
                case '\'':
                case '"':
                    ScanString(ref token);
                    break;
                case ' ':
                case '\t':
                    ScanWhitespace(ref token);
                    break;

                //TODO: Treat newlines as their own token, not text
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                    ScanIdentifier(ref token);
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ScanNumber(ref token);
                    break;
                default:
                    throw new Exception(String.Format("Unexpected character '{0}' ({1})", currentChar, (int)currentChar));
            }
            return token;
        }

        private void ScanString(ref Token token)
        {
            char quoteChar = _nextChar;
            char nextChar = Advance();
            int startPosition = _input.CurrentPosition;

            while (true)
            {
                switch (nextChar)
                {
                    case '\'':
                    case '"':
                        if (nextChar != quoteChar)
                            goto default;

                        token.Value = _input.GetRange(startPosition, _input.CurrentPosition -1);
                        token.TokenKind = quoteChar == '"'
                            ? TokenKind.InterpolatedStringLiteral
                            : TokenKind.StringLiteral;
                        Advance();
                        return;
                    case '\r':
                    case '\n':
                        throw new Exception("Unexpected newline in string");
                    case '\0':
                        throw new Exception("Unexpected end of file in string");
                    default:
                        _builder.Append(nextChar);
                        nextChar = Advance();
                        break;
                }
            }
        }

        private void ScanNumber(ref Token token)
        {
            int startPosition = _input.CurrentPosition;

            var nextChar = _nextChar;
            while (true)
            {
                switch (nextChar)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        break;
                    default:
                        token.TokenKind = TokenKind.NumericLiteral;
                        token.Value = _input.GetRange(startPosition, _input.CurrentPosition -1);
                        return;

                }
                nextChar = Advance();
            }
        }


        private void ScanIdentifier(ref Token token)
        {
            int startPosition = _input.CurrentPosition;

            var nextChar = _nextChar;
            while (true)
            {
                switch (nextChar)
                {
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '_':
                    case '-':
                        break;
                    default:
                        token.TokenKind = TokenKind.Identifier;
                        token.Value = _input.GetRange(startPosition, _input.CurrentPosition -1);
                        return;
                }
                nextChar = Advance();
            }
        }

        private void ScanWhitespace(ref Token token)
        {
            int startPosition = _input.CurrentPosition;
            char nextChar = _nextChar;
            while(true)
            {
                switch (nextChar)
                {
                    case ' ':
                    case '\t':
                        nextChar = Advance();
                        break;
                    default:
                        token.TokenKind = TokenKind.Whitespace;
                        token.Value = _input.GetRange(startPosition, _input.CurrentPosition -1);
                        return;
                }
            }
        }

        private char Advance()
        {
            return _nextChar = _input.MoveNext();
        }
    }
}
