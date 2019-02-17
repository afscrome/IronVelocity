using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Parser{
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position;

        public Parser(IEnumerable<SyntaxToken> tokens)
            : this(tokens.ToImmutableArray()) { }

        public Parser(ImmutableArray<SyntaxToken> tokens)
        {
            _tokens = tokens;
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _tokens.Length)
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

        public ExpressionSyntax ParseExpression()
        {
            switch(Current.Kind)
            {
                case SyntaxKind.NumberToken:
                    var token = NextToken();
                    return new LiteralExpressionSyntax(token, token.Value);

                default:
                    throw new Exception($"Unexpected Token {Current.Kind}");
            }
        }
    }
}