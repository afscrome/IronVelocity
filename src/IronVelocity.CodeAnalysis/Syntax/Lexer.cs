using System.Collections.Generic;
using System.Collections.Immutable;
using IronVelocity.CodeAnalysis.Text;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Lexer
    {
        private readonly SourceText _text;
        public DiagnosticBag Diagnostics { get; } = new DiagnosticBag();
        private int _position;


        public Lexer(string text)
            : this(new SourceText(text))
        {
        }

        public Lexer(SourceText text)
        {
            _text = text;
        }

        public ImmutableArray<SyntaxToken> ReadAllTokens()
        {
            var builder = ImmutableArray.CreateBuilder<SyntaxToken>();

            while (true)
            {
                var token = NextToken();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;
                builder.Add(token);
            }

            return builder.ToImmutable();
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
            switch (Current)
            {
                case '\0':
                    return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "");

                case '$':
                    return BasicToken(SyntaxKind.DollarToken, "$");

                case '#' when LookAhead == '#':
                    return SingleLineComment();
                case '#' when LookAhead == '*':
                    return BlockComment();
                case '#' when LookAhead == '[' && Peek(2) == '[':
                    return Literal();
                case '#':
                    return BasicToken(SyntaxKind.HashToken, "#");
               
                case ' ':
                case '\t':
                    return HorizontalWhitesapce();

                case '\r':
                case '\n':
                    return VerticalWhitesapce();

                case '(':
                    return BasicToken(SyntaxKind.OpenParenthesisToken, "(");
                case ')':
                    return BasicToken(SyntaxKind.CloseParenthesisToken, ")");
                case '+':
                    return BasicToken(SyntaxKind.PlusToken, "+");
                case '-':
                    return BasicToken(SyntaxKind.MinusToken, "-");
                case '*':
                    return BasicToken(SyntaxKind.StarToken, "*");
                case '/':
                    return BasicToken(SyntaxKind.SlashToken, "/");
                case '%':
                    return BasicToken(SyntaxKind.ModuloToken, "%");

                case '=' when LookAhead == '=':
                    return BasicToken(SyntaxKind.EqualsEqualsToken, "==");
                case '!' when LookAhead == '=':
                    return BasicToken(SyntaxKind.BangEqualsToken, "!=");

                case '!':
                    return BasicToken(SyntaxKind.BangToken, "!");
                case '&' when LookAhead == '&':
                    return BasicToken(SyntaxKind.AmpersandAmpersandToken, "&&");
                case '|' when LookAhead == '|':
                    return BasicToken(SyntaxKind.PipePipeToken, "||");

                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    return Number();

                case var c when char.IsLetter(c):
                    return Keyword();

                default:
                    Diagnostics.ReportBadCharacter(_position, Current);
                    return BasicToken(SyntaxKind.BadToken, Current.ToString());
            }
        }

        private SyntaxToken BasicToken(SyntaxKind kind, string text)
        {
            int startPosition = _position;
            _position += text.Length;
            return new SyntaxToken(kind, startPosition, text);
        }

        private SyntaxToken TokenSincePosition(SyntaxKind kind, int start)
            => new SyntaxToken(kind, start, TextSincePosition(start));

        private string TextSincePosition(int start)
            => _text.Substring(start, _position - start);

        private SyntaxToken Number()
        {
            int start = _position;

            while (Current >= '0' && Current <='9')
            {
                _position++;
            }

            var text = TextSincePosition(start);

            object? value = null;
            if (int.TryParse(text, out int number))
            {
                value = number;
            }
            else
            {
                Diagnostics.ReportInvalidNumber(start, text);
            }

            return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
        }

        private SyntaxToken Keyword()
        {
            int start = _position;

            while (char.IsLetter(Current))
            {
                _position++;
            }

            var text = TextSincePosition(start);
            var kind = SyntaxFacts.GetKeywordKind(text);

            return new SyntaxToken(kind, start, text);
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

            return TokenSincePosition(SyntaxKind.LiteralToken, start);
        }

        private SyntaxToken SingleLineComment()
        {
            int start = _position;
            _position += 2;
            while (Current != '\r' && Current != '\n' && Current != '\0')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.SingleLineCommentToken, start);
        }


        private SyntaxToken BlockComment()
        {
            int start = _position;
            _position += 2;
            while (!(Current == '*' && LookAhead == '#') && LookAhead != '\0')
            {
                _position++;
            }

            if (LookAhead == '\0')
            {
                return TokenSincePosition(SyntaxKind.BadToken, start);
            }

            _position += 2;
            return TokenSincePosition(SyntaxKind.BlockCommentToken, start);
        }

        private SyntaxToken HorizontalWhitesapce()
        {
            int start = _position;
            _position++;
            while (Current == ' ' || Current == '\t')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.HorizontalWhitespaceToken, start);
        }

        private SyntaxToken VerticalWhitesapce()
        {
            int start = _position;
            _position++;
            while (Current == '\n' || Current == '\r')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.VerticalWhitespaceToken, start);
        }
    }

}