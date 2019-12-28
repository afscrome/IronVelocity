using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Parser
    {
        private readonly IImmutableList<SyntaxToken> _tokens;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private int _position;

        public Parser(IEnumerable<SyntaxToken> tokens)
        {
            //This is an ugly hack - need a better long term solution for handling whitespace like this
            _tokens = tokens
                .Where(x => x.Kind != SyntaxKind.HorizontalWhitespaceToken)
                .ToImmutableArray();
        }


        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _tokens.Count)
                return _tokens.Last();

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
                _diagnostics.ReportUnexpectedToken(Current, kind);
                return new SyntaxToken(kind, Current.Position, Current.Text);
            }
        }

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_diagnostics.Diagnostics, expression);
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
                    _diagnostics.ReportUnexpectedToken(Current, SyntaxKind.FalseKeyword, SyntaxKind.NumberToken, SyntaxKind.OpenParenthesisToken, SyntaxKind.TrueKeyword);
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