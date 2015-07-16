using IronVelocity.Parser.AST;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace IronVelocity.Parser
{
    public class VelocityParser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;

        public bool HasReachedEndOfFile { get { return CurrentToken.TokenKind == TokenKind.EndOfFile; } }

        private Token CurrentToken { get { return _currentToken ?? (_currentToken = _lexer.GetNextToken()); } }

        public VelocityParser(string input)
            : this(input, LexerState.Text)
        {
        }

        public VelocityParser(string input, LexerState state)
        {
            _lexer = new Lexer(input, state);
            MoveNext();
        }


        private void MoveNext()
        {
            _currentToken = null;
        }

        public RenderedOutputNode Parse()
        {
            var children = ImmutableList.CreateBuilder<SyntaxNode>();


            var textSoFar = new StringBuilder();
            while (true)
            {
                var token = CurrentToken;
                switch (token.TokenKind)
                {
                    case TokenKind.Text:
                        textSoFar.Append(token.Value);
                        Eat(TokenKind.Text);
                        break;
                    case TokenKind.Dollar:
                        _lexer.State = LexerState.Vtl;
                        ReferenceOrText(textSoFar, children);
                        _lexer.State = LexerState.Text;
                        break;
                    case TokenKind.EndOfFile:
                        if (textSoFar.Length > 0)
                        {
                            children.Add(new TextNode(textSoFar.ToString()));
                            textSoFar.Clear();
                        }
                        return new RenderedOutputNode(children.ToImmutableArray());
                    case TokenKind.Hash:
                        if (textSoFar.Length > 0)
                        {
                            children.Add(new TextNode(textSoFar.ToString()));
                            textSoFar.Clear();
                        }
                        children.Add(Directive());
                        break;
                    default:
                        textSoFar.Append(token.GetValue());
                        MoveNext();
                        break;
                }
            }
        }


        protected virtual void ReferenceOrText(StringBuilder textSoFar, ImmutableList<SyntaxNode>.Builder nodeBuilder)
        {
            Eat(TokenKind.Dollar);

            while (TryEat(TokenKind.Dollar))
            {
                textSoFar.Append('$');
            }

            bool isSilent = TryEat(TokenKind.Exclamation);
            bool isFormal = TryEat(TokenKind.LeftCurley);

            if (CurrentToken.TokenKind != TokenKind.Identifier)
            {
                textSoFar.Append('$');
                if (isSilent)
                    textSoFar.Append('!');
                if (isFormal)
                    textSoFar.Append('{');

                return;
            }

            var identifier = Eat(TokenKind.Identifier);
            if (textSoFar.Length > 0)
                nodeBuilder.Add(new TextNode(textSoFar.ToString()));

            //If we have a formal reference, we don't expect any text until the next right curley brace

            if (isFormal)
                nodeBuilder.Add(ReferenceInternal(isSilent, isFormal, identifier));
            else
            {
                textSoFar.Clear();
                ReferenceNodePart value = Variable(identifier);

                textSoFar.Clear();
                while (textSoFar.Length == 0 && TryEat(TokenKind.Dot))
                {
                    identifier = CurrentToken;
                    if (!TryEat(TokenKind.Identifier))
                    {
                        textSoFar.Append('.');
                        textSoFar.Append(identifier.GetValue());
                        break;
                    }

                    if (TryEat(TokenKind.LeftParenthesis))
                    {
                        textSoFar.Append("(");
                        //This should match the tokens allowed to start an Expression in Expression()
                        switch (CurrentToken.TokenKind)
                        {
                            case TokenKind.LeftParenthesis:
                            //case TokenKind.Exclamation: //This is allowed in NVelocity
                            case TokenKind.Dollar:
                            case TokenKind.Minus:
                            case TokenKind.NumericLiteral:
                            case TokenKind.StringLiteral:
                            case TokenKind.InterpolatedStringLiteral:
                            case TokenKind.Identifier:
                            case TokenKind.LeftSquareBracket:
                            case TokenKind.EndOfFile:
                                value = Property(value, identifier);
                                textSoFar.Append(CurrentToken.GetValue());
                                goto end;
                            default:
                                value = Method(value, identifier);
                                break;
                        }
                    }
                    else
                    {
                        value = Property(value, identifier);
                    }
                }

            end:
                var reference = new ReferenceNode(value, isSilent, isFormal);
                nodeBuilder.Add(reference);
            }

        }

        protected virtual SyntaxNode Directive()
        {
            Eat(TokenKind.Hash);
            _lexer.State = LexerState.Vtl;
            var identifier = CurrentToken;
            if (!TryEat(TokenKind.Identifier))
                return new DirectiveNode(String.Empty);

            var name = identifier.Value;

            SyntaxNode result;
            if (name == "set")
            {
                result = SetDirective();
            }
            else
            {
                result = new DirectiveNode(name);
            }
            _lexer.State = LexerState.Text;

            return result;
        }

        public virtual BinaryExpressionNode SetDirective()
        {
            Eat(TokenKind.LeftParenthesis);
            var left = Expression();
            Eat(TokenKind.Equals);
            var right = CompoundExpression();
            Eat(TokenKind.RightParenthesis);
            _lexer.State = LexerState.Text;

            return new BinaryExpressionNode(BinaryOperation.Assignment, left, right);
        }


        protected virtual SyntaxNode ReferenceOrText()
        {
            Eat(TokenKind.Dollar);

            if (CurrentToken.TokenKind == TokenKind.Dollar)
            {
                return new TextNode("$");
            }

            bool isSilent = TryEat(TokenKind.Exclamation);
            bool isFormal = TryEat(TokenKind.LeftCurley);

            var possibleIdentifier = CurrentToken;
            if (!TryEat(TokenKind.Identifier))
            {
                var text = "$";
                if (isSilent)
                    text += "!";
                if (isFormal)
                    text += "{";

                return new TextNode(text);
            }

            return ReferenceInternal(isSilent, isFormal, possibleIdentifier);
        }


        protected virtual ReferenceNode Reference()
        {

            Eat(TokenKind.Dollar);
            bool isSilent = TryEat(TokenKind.Exclamation);
            bool isFormal = TryEat(TokenKind.LeftCurley);

            var identifier = Eat(TokenKind.Identifier);
            return ReferenceInternal(isSilent, isFormal, identifier);
        }

        private ReferenceNode ReferenceInternal(bool isSilent, bool isFormal, Token identifier)
        {
            //Root variable
            ReferenceNodePart value = Variable(identifier);

            //Methods & Properties
            while (TryEat(TokenKind.Dot))
            {
                identifier = Eat(TokenKind.Identifier);

                if (TryEat(TokenKind.LeftParenthesis))
                {
                    value = Method(value, identifier);
                }
                else
                {
                    value = Property(value, identifier);
                }
            }

            if (isFormal)
            {
                Eat(TokenKind.RightCurley);
            }

            return new ReferenceNode(value, isSilent, isFormal);
        }

        protected virtual Variable Variable(Token identifier)
        {
            return new Variable(identifier.Value);
        }

        protected virtual Method Method(ReferenceNodePart target, Token identifier)
        {
            var arguments = ArgumentList(TokenKind.RightParenthesis);
            return new Method(identifier.Value, target, arguments);
        }

        protected virtual Property Property(ReferenceNodePart target, Token identifier)
        {
            return new Property(identifier.Value, target);
        }

        private void AddRemainingArguments(ICollection<ExpressionNode> arguments, TokenKind closingToken)
        {
            if (!TryEat(closingToken))
            {
                do
                {
                    arguments.Add(Expression());
                }
                while (TryEat(TokenKind.Comma));
                Eat(closingToken);
            }
        }

        public ArgumentsNode ArgumentList(TokenKind closingToken)
        {
            TryEatWhitespace();

            var builder = ImmutableList.CreateBuilder<ExpressionNode>();
            AddRemainingArguments(builder, closingToken);

            return new ArgumentsNode(builder.ToImmutableArray());
        }

        protected virtual ExpressionNode Number()
        {
            bool isNegative = TryEat(TokenKind.Minus);

            var numberToken = Eat(TokenKind.NumericLiteral);

            var integerPart = isNegative
                ? "-" + numberToken.Value
                : numberToken.Value;

            if (TryEat(TokenKind.Dot))
                return FloatingPointLiteral(integerPart);
            else
                return IntegerLiteral(integerPart);
        }

        protected virtual IntegerLiteralNode IntegerLiteral(string integerPart)
        {
            var integerValue = int.Parse(integerPart);
            return new IntegerLiteralNode(integerValue);
        }

        protected virtual FloatingPointLiteralNode FloatingPointLiteral(string integerPart)
        {
            var numberToken = Eat(TokenKind.NumericLiteral);
            var fractionalPart = numberToken.Value;

            var floatValue = float.Parse(integerPart + "." + fractionalPart);
            return new FloatingPointLiteralNode(floatValue);

        }

        protected virtual StringNode StringLiteral()
        {
            var token = Eat(TokenKind.StringLiteral);
            return new StringNode(token.Value, false);
        }

        protected virtual StringNode InterpolatedString()
        {
            var token = Eat(TokenKind.InterpolatedStringLiteral);
            return new StringNode(token.Value, true);
        }

        protected virtual UnaryExpressionNode Not()
        {
            Eat(TokenKind.Exclamation);

            var target = Expression();
            return new UnaryExpressionNode(UnaryOperation.Not, target);

        }

        protected virtual UnaryExpressionNode Parenthesised()
        {
            Eat(TokenKind.LeftParenthesis);
            var target = Expression();
            var result = new UnaryExpressionNode(UnaryOperation.Parenthesised, target);
            Eat(TokenKind.RightParenthesis);
            return result;
        }

        public virtual ExpressionNode Expression()
        {
            TryEatWhitespace();

            ExpressionNode result;
            //N.B. any additions here should also be added to ReferenceOrText
            switch (CurrentToken.TokenKind)
            {
                case TokenKind.LeftParenthesis:
                    result = Parenthesised();
                    break;
                case TokenKind.Exclamation:
                    result = Not();
                    break;
                case TokenKind.Dollar:
                    result = Reference();
                    break;
                case TokenKind.Minus: //Negative numbers
                case TokenKind.NumericLiteral:
                    result = Number();
                    break;
                case TokenKind.StringLiteral:
                    result = StringLiteral();
                    break;
                case TokenKind.InterpolatedStringLiteral:
                    result = InterpolatedString();
                    break;
                case TokenKind.Identifier:
                    result = BooleanLiteralOrWord();
                    break;
                case TokenKind.LeftSquareBracket:
                    result = RangeOrList();
                    break;
                case TokenKind.EndOfFile:
                    throw new Exception("Unexpected end of file");
                case TokenKind.LeftCurley: //Dictionary

                    throw new NotImplementedException(String.Format("Can't yet parse token {0} starting an expression", CurrentToken.TokenKind));
                default:
                    throw new Exception("Unrecognised token parsing an expression: " + CurrentToken.TokenKind);
            }

            TryEatWhitespace();

            return result;
        }

        private static bool HasHigherPrecedence(BinaryOperation left, BinaryOperation right)
        {
            left &= (BinaryOperation)0xFFFF0000;
            right &= (BinaryOperation)0xFFFF0000;

            return left > right;
        }

        public virtual ExpressionNode CompoundExpression()
        {
            var soFar = Expression();
            BinaryExpressionNode soFarBinary = null;
            while (true)
            {
                TokenKind tokenKind = CurrentToken.TokenKind;
                BinaryOperation operation;
                switch (tokenKind)
                {
                    case TokenKind.Plus:
                        operation = BinaryOperation.Addition;
                        break;
                    case TokenKind.Minus:
                        operation = BinaryOperation.Subtraction;
                        break;
                    case TokenKind.Multiply:
                        operation = BinaryOperation.Multiplication;
                        break;
                    case TokenKind.Divide:
                        operation = BinaryOperation.Division;
                        break;
                    case TokenKind.Modulo:
                        operation = BinaryOperation.Modulo;
                        break;
                    default:
                        return soFar;
                }
                Eat(tokenKind);
                var operand = Expression();

                if (soFarBinary != null && HasHigherPrecedence(operation, soFarBinary.Operation))
                {
                    var right = new BinaryExpressionNode(operation, soFarBinary.Right, operand);
                    soFar = new BinaryExpressionNode(soFarBinary.Operation, soFarBinary.Left, right);
                }
                else
                {
                    soFar = soFarBinary = new BinaryExpressionNode(operation, soFar, operand);
                }

            }
        }

        protected virtual ExpressionNode BooleanLiteralOrWord()
        {
            var token = Eat(TokenKind.Identifier);
            var value = token.Value;
            if (value == "true")
                return BooleanLiteralNode.True;
            else if (value == "false")
                return BooleanLiteralNode.False;
            else
                return new WordNode(value);
        }

        protected virtual ExpressionNode RangeOrList()
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

            AddRemainingArguments(builder, TokenKind.RightSquareBracket);

            return new ListExpressionNode(builder.ToImmutableArray());
        }

        private BinaryExpressionNode Range(ExpressionNode left)
        {
            var right = Expression();
            Eat(TokenKind.RightSquareBracket);

            return new BinaryExpressionNode(BinaryOperation.Range, left, right);
        }

        private Token Eat(TokenKind tokenKind)
        {
            var token = CurrentToken;
            if (CurrentToken.TokenKind != tokenKind)
            {
                throw UnexpectedTokenException(tokenKind);
            }
            MoveNext();
            return token;
        }

        private bool TryEat(TokenKind tokenKind)
        {
            if (CurrentToken.TokenKind == tokenKind)
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
            return new Exception(String.Format("Unexpected Token {0}. Expected {1}.", CurrentToken.TokenKind, expectedToken));
        }

        private Exception UnexpectedTokenException(params TokenKind[] expectedTokenKinds)
        {
            //TODO: Throw specific exception type, include line & column data.
            return new Exception(String.Format("Unexpected Token {0}. Expected one of: {1}.", CurrentToken.TokenKind, String.Join(",", expectedTokenKinds)));
        }

        private bool TryEatWhitespace()
        {
            return TryEat(TokenKind.Whitespace);
        }


    }

}
