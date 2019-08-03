using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Parser
    {
        private readonly IImmutableList<SyntaxToken> _tokens;
        private int _position;

        public IImmutableList<string> Diagnostics { get; private set; } = ImmutableList<string>.Empty;

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

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(Diagnostics, expression);
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
                case SyntaxKind.NumberToken:
                    var numberToken = NextToken();
                    return new LiteralExpressionSyntax(numberToken, numberToken.Value);
                case SyntaxKind.OpenParenthesisToken:
                    var open = NextToken();
                    var expression = ParseExpression();
                    var close = Match(SyntaxKind.CloseParenthesisToken);
                    return new ParenthesisedExpressionSyntax(open, expression, close);
                default:
                    //TODO: Better error handling
                    ReportError($"ERROR: Unexpected Token <{Current.Kind}>, expected <NumberToken> or <OpenParenthesisToken>");
                    return new LiteralExpressionSyntax(new SyntaxToken(SyntaxKind.BadToken, Current.Position, null), null);
            }
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return NextToken();
            }
            else
            {
                ReportError($"ERROR: Unexpected token <{Current.Kind}>, expected <{kind}>");
                return new SyntaxToken(kind, Current.Position, null);
            }
        }

        private void ReportError(string message)
        {
            Diagnostics = Diagnostics.Add(message);
        }

    }
}