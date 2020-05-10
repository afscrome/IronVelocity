using IronVelocity.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Parser
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly IImmutableList<SyntaxToken> _tokens;
        private int _position;

        public Parser(SourceText text)
        {
            var tokens = ImmutableArray.CreateBuilder<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();

                if (token.Kind != SyntaxKind.HorizontalWhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);


            _tokens = tokens.ToImmutable();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        public DiagnosticBag Diagnostics { get; } = new DiagnosticBag();

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _tokens.Count)
            {
                var lastToken = _tokens.Last();
                return new SyntaxToken(SyntaxKind.EndOfFileToken, lastToken.Span.End, "", null);
            }

            return _tokens[index];
        }

        public SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return NextToken();
            }
            else
            {
                Diagnostics.ReportUnexpectedToken(Current, kind);
                return new SyntaxToken(kind, Current.Position, Current.Text);
            }
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var expression = ParseExpression();
            Match(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(expression);
        }

        public ExpressionSyntax ParseExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(Current.Kind);
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence )
            {
                left = new UnaryExpressionSyntax(operatorToken: NextToken(),
                    operand: ParseExpression(unaryOperatorPrecedence));
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                var precedence = SyntaxFacts.GetBinaryOperatorPrecedence(Current.Kind);

                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }
                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            return left;
        }

        public ExpressionSyntax ParsePrimaryExpression()
        {
            switch(Current.Kind)
            {
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteral(false);

                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();

                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesisedExpression();

                case SyntaxKind.TrueKeyword:
                    return ParseBooleanLiteral(true);

                default:
                    //TODO: Better error handling
                    Diagnostics.ReportUnexpectedToken(Current, SyntaxKind.FalseKeyword, SyntaxKind.NumberToken, SyntaxKind.OpenParenthesisToken, SyntaxKind.TrueKeyword);
                    return new LiteralExpressionSyntax(new SyntaxToken(SyntaxKind.BadToken, Current.Position, Current.Text), null);
            }

        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken, numberToken.Value);
        }

        private ExpressionSyntax ParseParenthesisedExpression()
        {
            var open = Match(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var close = Match(SyntaxKind.CloseParenthesisToken);
            return new ParenthesisedExpressionSyntax(open, expression, close);
        }

        private ExpressionSyntax ParseBooleanLiteral(bool value)
        {
            var keywordToken = NextToken();
            return new LiteralExpressionSyntax(keywordToken, value);
        }
    }
}