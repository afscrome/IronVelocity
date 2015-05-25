using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser
{
    public class Parser
    {
        private readonly Lexer _lexer;
        private Token _nextToken;
        private Token _currentToken;

        public Parser(string input)
        {
            _lexer = new Lexer(input);
        }

        private Token Peek()
        {
            return _nextToken ?? (_nextToken = _lexer.GetNextToken());
        }


        private Token MoveNextAndPeek()
        {
            _currentToken = _nextToken ?? _lexer.GetNextToken();
            _nextToken = null;
            return Peek();
        }


        public Reference Reference()
        {
            Token token = Peek();

            if (token.TokenKind != TokenKind.Dollar)
            {
                throw new Exception("TODO: 0");
            }
            token = MoveNextAndPeek();

            bool isSilent = token.TokenKind == TokenKind.Exclamation;
            if (isSilent)
            {
                token = MoveNextAndPeek();
            }

            bool isFormal = token.TokenKind == TokenKind.LeftCurley;
            if (isFormal)
            {
                token = MoveNextAndPeek();
            }

            if (token.TokenKind != TokenKind.Identifier)
            {
                //TODO: Backout??
                throw new Exception("TODO: 1");
            }
            var variableName = token.Value;

            //TODO: Property, Method, Indexers

            token = MoveNextAndPeek();
            if (isFormal && token.TokenKind != TokenKind.RightCurley)
            {
                //TODO: Backout
                throw new Exception("TODO: 2");
            }

            return new Reference
            {
                IsSilent = isSilent,
                IsFormal = isFormal,
                Identifier = variableName
            };

        }
    }

    public class Reference : Expression
    {
        public bool IsSilent { get; set; }
        public bool IsFormal { get; set; }
        public string Identifier { get; set; }

    }
}
