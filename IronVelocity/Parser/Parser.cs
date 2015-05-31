using IronVelocity.Parser.AST;
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
        private Token _currentToken;

        public Parser(string input)
        {
            _lexer = new Lexer(input);
            _currentToken = _lexer.GetNextToken();
        }

        public bool HasReachedEndOfFile { get { return _currentToken.TokenKind == TokenKind.EndOfFile; } }

        private Token MoveNext()
        {
            return _currentToken = _lexer.GetNextToken();
        }


        public ReferenceNode Reference()
        {
            //Get Metadata about this reference
            Token token = _currentToken;

            if (token.TokenKind != TokenKind.Dollar)
            {
                throw new Exception("TODO: 0");
            }
            token = MoveNext();

            bool isSilent = token.TokenKind == TokenKind.Exclamation;
            if (isSilent)
            {
                token = MoveNext();
            }

            bool isFormal = token.TokenKind == TokenKind.LeftCurley;
            if (isFormal)
            {
                token = MoveNext();
            }

            if (token.TokenKind != TokenKind.Identifier)
            {
                //TODO: Backout??
                throw new Exception("TODO: 1");
            }

            //Root variable
            ReferenceInnerNode value = new Variable {
                Name = token.Value
            };

            //Methods & Properties
            token = MoveNext();
            while (token.TokenKind == TokenKind.Dot)
            {
                token = MoveNext();
                if (token.TokenKind != TokenKind.Identifier)
                {
                    throw new Exception("TODO: 3");
                }
                var name = token.Value;
                token = MoveNext();
                if (token.TokenKind == TokenKind.LeftParenthesis)
                {
                    var args = Arguments();
                    value = new Method { Name = name, Target = value, Arguments = args };
                    token = _currentToken;
                }
                else
                {
                    value = new Property { Name = name, Target = value };
                }
            }

            if (isFormal)
            {
                if (token.TokenKind == TokenKind.RightCurley)
                {
                    MoveNext();
                }
                else
                {
                    //TODO: Backout
                    throw new Exception("TODO: Formal Reference Problem");
                }
            }

            return new ReferenceNode
            {
                IsSilent = isSilent,
                IsFormal = isFormal,
                Value = value,
            };

        }

        public ArgumentsNode Arguments()
        {
            if (_currentToken.TokenKind != TokenKind.LeftParenthesis)
            {
                throw new Exception("Expected '('");
            }
            
            MoveNext();
            var args = new List<ExpressionNode>();
            while (true)
            {
                if (_currentToken.TokenKind == TokenKind.Whitespace)
                    MoveNext();

                if (_currentToken.TokenKind == TokenKind.RightParenthesis)
                {
                    break;
                }

                args.Add(Expression());

                if (_currentToken.TokenKind == TokenKind.RightParenthesis)
                    break;

                if (_currentToken.TokenKind != TokenKind.Comma)
                    throw new Exception("Expected ',' or ')'");
                MoveNext();
            }
            MoveNext();
            return new ArgumentsNode { Arguments = args };

        }

        public NumericNode Number()
        {
            var token = _currentToken;
            bool isNegative = token.TokenKind == TokenKind.Dash;
            if (isNegative)
                token = MoveNext();

            if (token.TokenKind != TokenKind.Number)
                throw new Exception("Expected number");

            var integerPart = isNegative
                ? "-" + token.Value
                : token.Value;

            token = MoveNext();
            if (token.TokenKind != TokenKind.Dot)
            {
                return new NumericNode { Value = integerPart };
            }

            token = MoveNext();
            if (token.TokenKind != TokenKind.Number)
                throw new Exception("Expected number");
            var fractionalPart = token.Value;

            MoveNext();
            return new NumericNode { Value = integerPart + "." + fractionalPart };
        }

        public ExpressionNode Expression()
        {
            var currentToken = _currentToken;

            if (currentToken.TokenKind == TokenKind.Whitespace)
                currentToken = MoveNext();

            ExpressionNode result;
            switch (_currentToken.TokenKind)
            {
                case TokenKind.Dollar:
                    result = Reference();
                    break;
                case TokenKind.Dash:
                case TokenKind.Number:
                    result = Number();
                    break;

                case TokenKind.EndOfFile:
                    throw new Exception("Unexpected end of file");
                case TokenKind.Exclamation: //Not
                case TokenKind.LeftParenthesis: //Operator precedence
                case TokenKind.LeftCurley: //Dictionary
                    throw new NotImplementedException(String.Format("Can't yet parse token {0} starting an expression", _currentToken.TokenKind));
                default:
                    throw new Exception("Unrecognised token parsing an expression: " + _currentToken.TokenKind);
            }

            if (_currentToken.TokenKind == TokenKind.Whitespace)
                MoveNext();

            return result;
        }


        

    }

}
