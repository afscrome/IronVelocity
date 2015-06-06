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
                IgnoreWhitespace();

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

        public ExpressionNode Number()
        {
            var token = _currentToken;
            bool isNegative = token.TokenKind == TokenKind.Dash;
            if (isNegative)
                token = MoveNext();

            if (token.TokenKind != TokenKind.NumericLiteral)
                throw new Exception("Expected number");

            var integerPart = isNegative
                ? "-" + token.Value
                : token.Value;

            token = MoveNext();
            int intValue;
            if (token.TokenKind != TokenKind.Dot && int.TryParse(integerPart, out intValue))
            {
                return new IntegerNode { Value = intValue };
            }

            token = MoveNext();
            if (token.TokenKind != TokenKind.NumericLiteral)
                throw new Exception("Expected number");
            var fractionalPart = token.Value;

            MoveNext();
            var floatValue = float.Parse(integerPart + "." + fractionalPart);
            return new FloatingPointNode { Value = floatValue };
        }

        public ExpressionNode Expression()
        {
            var currentToken = _currentToken;

            currentToken = IgnoreWhitespace();

            ExpressionNode result;
            switch (currentToken.TokenKind)
            {
                case TokenKind.Dollar:
                    result = Reference();
                    break;
                case TokenKind.Dash:
                case TokenKind.NumericLiteral:
                    result = Number();
                    break;
                case TokenKind.StringLiteral:
                    result = new StringNode { Value = currentToken.Value, IsInterpolated = false };
                    MoveNext();
                    break;
                case TokenKind.InterpolatedStringLiteral:
                    result = new StringNode { Value = currentToken.Value, IsInterpolated = true };
                    MoveNext();
                    break;

                case TokenKind.Identifier:
                    var value = currentToken.Value;
                    if (value == "true")
                        result = BooleanNode.True;
                    else if (value == "false")
                        result = BooleanNode.False;
                    else
                        result = new WordNode { Name = value };
                    MoveNext();
                    break;

                case TokenKind.LeftSquareBracket:
                    result = Range();
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

            IgnoreWhitespace();

            return result;
        }

        private BinaryExpressionNode Range()
        {
            Eat(TokenKind.LeftSquareBracket);

            var left = Expression();

            Eat(TokenKind.DotDot);
            var right = Expression();

            Eat(TokenKind.RightSquareBracket);

            return new BinaryExpressionNode { Left = left, Right = right, Operation = BinaryOperation.Range };
        }

        private Token Eat(TokenKind tokenKind)
        {
            var token = _currentToken;
            if (token.TokenKind != tokenKind)
            {
                throw new Exception("Expected Token Kind: " + tokenKind);
            }
            MoveNext();
            return token;
        }


        private Token IgnoreWhitespace()
        {
            return _currentToken.TokenKind == TokenKind.Whitespace
                ? MoveNext()
                : _currentToken;
        }
        

    }

}
