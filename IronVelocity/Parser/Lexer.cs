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
        private int _nextPosition = 0;
        private StringBuilder _builder = new StringBuilder();

        public Lexer(string input)
        {
            _input = input;
        }

        public Token GetNextToken()
        {
            var token = new Token();

            char peek = Peek();
            switch (peek)
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
                    Advance();
                    ScanIdentifier(ref token, peek);
                    break;
                default:
                    throw new NotSupportedException();
                    break;
            }
            return token;
        }

        private void ScanIdentifier(ref Token token, char firstChar)
        {
            _builder.Clear();
            _builder.Append(firstChar);

            while (true)
            {
                var peek = Peek();
                switch (peek)
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
                        _builder.Append(peek);
                        Advance();
                        break;
                    default:
                        token.TokenKind = TokenKind.Identifier;
                        token.Value = _builder.ToString();
                        return;
                }
            }
        }

        private char Peek()
        {
            if (_nextPosition >= _input.Length)
                return default(char);

            return _input[_nextPosition];
        }

        private void Advance()
        {
            _nextPosition++;
        }
    }
}
