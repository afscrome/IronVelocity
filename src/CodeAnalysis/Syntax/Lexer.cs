
using System;
using IronVelocity.CodeAnalysis.Text;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Lexer
    {
        private readonly SourceText _text;
        private int _position;

        public Lexer(string text)
        {
            _text = new SourceText(text);
        }

        private char Current => Peek(0);
        private char LookAhead => Peek(1);

        private char Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }


        public SyntaxToken NextToken()
        {
            SyntaxKind kind;
            string text;
            int startPosition = _position;
            switch (Current)
            {
                case '\0':
                    kind = SyntaxKind.EndOfFile;
                    text = "\0";
                    break;

                case '$':
                    kind = SyntaxKind.Dollar;
                    text = "$";
                    break;

                case '#':
                    if (LookAhead == '#')
                        return SingleLineComment();
                    else if (LookAhead == '*')
                        return BlockComment();
                    else if (LookAhead == '[' && Peek(2) == '[')
                        return Literal();
                    else
                    {
                        kind = SyntaxKind.Hash;
                        text = "#";
                        break;
                    }
                
                case ' ':
                case '\t':
                    return HorizontalWhitesapce();

                case '\r':
                case '\n':
                    return VerticalWhitesapce();

                default:
                    kind = SyntaxKind.BadToken;
                    text = Current.ToString();
                    break;
            }

            _position += text.Length;
            return new SyntaxToken(kind, startPosition, text);
        }

        private SyntaxToken TokenSincePosition(SyntaxKind kind, int start)
        {
            var length = _position - start;

            var commentText = _text.Substring(start, length);

            return new SyntaxToken(kind, start, commentText);
        }


        private SyntaxToken Literal()
        {
            int start = _position;
            _position += 3;
            while (!(Current == ']' && LookAhead == ']'))
            {
                _position++;
            }

            _position += 2;

            return TokenSincePosition(SyntaxKind.Literal, start);
        }

        private SyntaxToken SingleLineComment()
        {
            int start = _position;
            _position += 2;
            while (Current != '\r' && Current != '\n' && Current != '\0')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.SingleLineComment, start);
        }


        private SyntaxToken BlockComment()
        {
            int start = _position;
            _position += 2;
            while (!(Current == '*' && LookAhead == '#'))
            {
                _position++;
            }

            _position += 2;

            return TokenSincePosition(SyntaxKind.BlockComment, start);
        }

        private SyntaxToken HorizontalWhitesapce()
        {
            int start = _position;
            _position++;
            while (Current == ' ' || Current == '\t')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.HorizontalWhitespace, start);
        }

        private SyntaxToken VerticalWhitesapce()
        {
            int start = _position;
            _position++;
            while (Current == '\n' || Current == '\r')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.VerticalWhitespace, start);
        }
    }

}