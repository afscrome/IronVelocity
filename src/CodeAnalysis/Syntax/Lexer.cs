
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
                case '#' when LookAhead == '#':
                    return SingleLineComment();
                case '#' when LookAhead == '*':
                    return BlockComment();
                default:
                    kind = SyntaxKind.BadToken;
                    text = Current.ToString();
                    break;
            }

            _position += text.Length;
            return new SyntaxToken(kind, startPosition, text);
        }

        private SyntaxToken SingleLineComment()
        {
            int start = _position;
            _position += 2;
            while (Current != '\r' && Current != '\n' && Current != '\0')
            {
                _position++;
            }

            var length = _position - start;

            var commentText = _text.Substring(start, length);

            return new SyntaxToken(SyntaxKind.Comment, start, commentText);
        }

        private SyntaxToken BlockComment()
        {
            int start = _position;
            _position += 2;
            while (Current != '*' && LookAhead != '#')
            {
                _position++;
            }

            _position += 2;

            var length = _position - start;

            var commentText = _text.Substring(start, length);

            return new SyntaxToken(SyntaxKind.Comment, start, commentText);
        }

    }

}