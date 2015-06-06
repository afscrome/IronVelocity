using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser
{
    public class Lexer
    {
        private readonly string _input;
        private StringBuilder _builder = new StringBuilder();
        private int _position = -1;
        private char _nextChar;

        public Lexer(string input)
        {
            _input = input;
            _nextChar = Advance();
        }

        public Token GetNextToken()
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
                    token.TokenKind = TokenKind.Dot;
                    Advance();
                    break;
                case '(':
                    token.TokenKind = TokenKind.LeftParenthesis;
                    Advance();
                    break;
                case ')':
                    token.TokenKind = TokenKind.RightParenthesis;
                    Advance();
                    break;
                case ',':
                    token.TokenKind = TokenKind.Comma;
                    Advance();
                    break;
                case '-':
                    token.TokenKind = TokenKind.Dash;
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
            _builder.Clear();
            char quoteChar = _nextChar;
            bool isInterpolated = quoteChar == '"';

            char nextChar = Advance();
            while(true)
            {
                switch (nextChar)
                {
                    case '\'':
                    case '"':
                        if (nextChar != quoteChar)
                            goto default;

                        token.Value = _builder.ToString();
                        token.TokenKind = quoteChar == '"'
                            ? TokenKind.InterpolatedStringLiteral
                            : TokenKind.StringLiteral;
                        Advance();
                        return;
                    case '\r':
                    case '\n':
                        throw new Exception("Unexpected newline in string");
                    default:
                        _builder.Append(nextChar);
                        nextChar = Advance();
                        break;
                }
            }
        }

        private void ScanNumber(ref Token token)
        {
            _builder.Clear();

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
                        _builder.Append(nextChar);
                        break;
                    default:
                        token.TokenKind = TokenKind.NumericLiteral;
                        token.Value = _builder.ToString();
                        return;

                }
                nextChar = Advance();
            }
        }


        private void ScanIdentifier(ref Token token)
        {
            _builder.Clear();

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
                        _builder.Append(nextChar);
                        break;
                    default:
                        token.TokenKind = TokenKind.Identifier;
                        token.Value = _builder.ToString();
                        return;
                }
                nextChar = Advance();
            }
        }

        private void ScanWhitespace(ref Token token)
        {
            _builder.Clear();
            char nextChar = _nextChar;
            while(true)
            {
                switch (nextChar)
                {
                    case ' ':
                    case '\t':
                        _builder.Append(nextChar);
                        nextChar = Advance();
                        break;
                    default:
                        token.TokenKind = TokenKind.Whitespace;
                        token.Value = _builder.ToString();
                        return;
                }
            }
        }

        private char Advance()
        {
            _position++;
            if (_position >= _input.Length)
                return _nextChar = default(char);

            return _nextChar = _input[_position];
        }
    }
}
