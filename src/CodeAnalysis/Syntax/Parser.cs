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
            : this(tokens.ToImmutableArray()) { }

        public Parser(IImmutableList<SyntaxToken> tokens)
        {
            _tokens = tokens;
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

        public ExpressionSyntax ParseExpression()
        {
            var token = Match(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(token, token.Value);
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
                return new SyntaxToken(kind, Current.Position, null, null);
            }
        }

        private void ReportError(string message)
        {
            Diagnostics = Diagnostics.Add(message);
        }

    }
}