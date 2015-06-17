using IronVelocity.Parser.AST;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            Eat(TokenKind.Dollar);
            bool isSilent = TryEat(TokenKind.Exclamation);
            bool isFormal = TryEat(TokenKind.LeftCurley);

            var token = Eat(TokenKind.Identifier);

            //Root variable
            ReferenceInnerNode value = new Variable {
                Name = token.Value
            };

            //Methods & Properties
            while (TryEat(TokenKind.Dot))
            {
                token = Eat(TokenKind.Identifier);

                var name = token.Value;
                if (_currentToken.TokenKind == TokenKind.LeftParenthesis)
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
                Eat(TokenKind.RightCurley);
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
            Eat(TokenKind.LeftParenthesis);
            
            TryEatWhitespace();

            var args = new List<ExpressionNode>();
            if (!TryEat(TokenKind.RightParenthesis))
            {
                while (true)
                {
                    args.Add(Expression());

                    if (TryEat(TokenKind.RightParenthesis))
                        break;
                    else
                        Eat(TokenKind.Comma);
                }
            }

            return new ArgumentsNode { Arguments = args };
        }

        public ExpressionNode Number()
        {
            bool isNegative = TryEat(TokenKind.Dash);

            var numberToken = Eat(TokenKind.NumericLiteral);

            var integerPart = isNegative
                ? "-" + numberToken.Value
                : numberToken.Value;

            int intValue;
            if (!TryEat(TokenKind.Dot) && int.TryParse(integerPart, out intValue))
            {
                return new IntegerNode { Value = intValue };
            }

            numberToken = Eat(TokenKind.NumericLiteral);
            var fractionalPart = numberToken.Value;

            var floatValue = float.Parse(integerPart + "." + fractionalPart);
            return new FloatingPointNode { Value = floatValue };
        }

        public ExpressionNode Expression()
        {

            TryEatWhitespace();

            var currentToken = _currentToken;
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
                    result = RangeOrList();
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

            TryEatWhitespace();

            return result;
        }

        private ExpressionNode RangeOrList()
        {
            Eat(TokenKind.LeftSquareBracket);

            TryEatWhitespace();

            if (TryEat(TokenKind.RightSquareBracket))
                return new ListExpressionNode(new ExpressionNode[0]);

            var firstArg = Expression();

            if (TryEat(TokenKind.DotDot))
                return Range(firstArg);
            else if (TryEat(TokenKind.Comma))
                return List(firstArg);
            else if (TryEat(TokenKind.RightSquareBracket))
                return new ListExpressionNode(firstArg);
            else
                throw UnexpectedTokenException(TokenKind.DotDot, TokenKind.Comma, TokenKind.RightSquareBracket, TokenKind.Whitespace);
        }

        private ListExpressionNode List(ExpressionNode firstValue)
        {
            var builder = ImmutableList.CreateBuilder<ExpressionNode>();
            builder.Add(firstValue);

            do
            {
                builder.Add(Expression());
            }
            while (TryEat(TokenKind.Comma));

            TryEatWhitespace();
            if (!TryEat(TokenKind.RightSquareBracket))
                throw UnexpectedTokenException(TokenKind.Comma, TokenKind.RightSquareBracket, TokenKind.Whitespace);

            return new ListExpressionNode(builder.ToImmutableArray());
        }

        private BinaryExpressionNode Range(ExpressionNode left)
        {
            var right = Expression();
            Eat(TokenKind.RightSquareBracket);

            return new BinaryExpressionNode { Left = left, Right = right, Operation = BinaryOperation.Range };
        }

        private Token Eat(TokenKind tokenKind)
        {
            var token = _currentToken;
            if (token.TokenKind != tokenKind)
            {
                throw UnexpectedTokenException(tokenKind);
            }
            MoveNext();
            return token;
        }

        private bool TryEat(TokenKind tokenKind)
        {
            var currentToken = _currentToken;
            if (currentToken.TokenKind == tokenKind)
            {
                MoveNext();
                return true;
            }
            else
            {
                return false;
            }
        }

        private Exception UnexpectedTokenException(TokenKind expectedToken)
        {
            //TODO: Throw specific exception type, include line & column data.
            return new Exception(String.Format("Unexpected Token {0}. Expected {1}.", _currentToken.TokenKind, expectedToken));
        }

        private Exception UnexpectedTokenException(params TokenKind[] expectedTokenKinds)
        {
            //TODO: Throw specific exception type, include line & column data.
            return new Exception(String.Format("Unexpected Token {0}. Expected one of: {1}.", _currentToken.TokenKind, String.Join(",", expectedTokenKinds)));
        }

        private bool TryEatWhitespace()
        {
            return TryEat(TokenKind.Whitespace);
        }
        

    }

}
