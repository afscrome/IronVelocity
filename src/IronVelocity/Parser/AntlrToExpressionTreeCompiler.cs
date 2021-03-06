﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IronVelocity.Compilation.AST;
using Antlr4.Runtime;
using IronVelocity.Compilation;
using IronVelocity.Binders;
using System.Linq;
using IronVelocity.Directives;
using System.Text;
using System.Dynamic;
using System.Collections.Immutable;
using IronVelocity.Reflection;

namespace IronVelocity.Parser
{
    public class AntlrToExpressionTreeCompiler : IVelocityParserVisitor<Expression>
    {
        private readonly AntlrVelocityParser _parser;
        private readonly IImmutableList<CustomDirectiveBuilder> _customDirectives;
        private readonly VelocityExpressionFactory _expressionFactory;

		public bool ReduceWhitespace { get; }

        public AntlrToExpressionTreeCompiler(AntlrVelocityParser parser, IImmutableList<CustomDirectiveBuilder> customDirectives, VelocityExpressionFactory expressionFactory, bool reduceWhitespace)
        {
            _parser = parser;
            _customDirectives = customDirectives ?? ImmutableList<CustomDirectiveBuilder>.Empty;
            _expressionFactory = expressionFactory;
			ReduceWhitespace = reduceWhitespace;
        }

        public Expression Visit(IParseTree tree) => tree.Accept(this);

        public Expression VisitTemplate([NotNull] VelocityParser.TemplateContext context) => Visit(context.block());

        public Expression VisitBlock([NotNull] VelocityParser.BlockContext context)
        {
            return new RenderedBlock(
                VisitMany(context.GetRuleContexts<ParserRuleContext>())
                );
        }

        public Expression VisitBlockComment([NotNull] VelocityParser.BlockCommentContext context) => Constants.EmptyExpression;

        public Expression VisitComment([NotNull] VelocityParser.CommentContext context) => Constants.EmptyExpression;

        private readonly StringBuilder _textBuffer = new StringBuilder();


		public Expression VisitText([NotNull] VelocityParser.TextContext context)
		{
			var rules = context.GetRuleContexts<ParserRuleContext>();
			_textBuffer.Clear();

			foreach (var rule in rules)
			{
				switch (rule.RuleIndex)
				{
					case VelocityParser.RULE_rawText:
						for (int i = 0; i < rule.ChildCount; i++)
						{
							var token = ((ITerminalNode)rule.GetChild(i)).Symbol;

							switch (token.Type)
							{
								case VelocityLexer.EscapedDollar:
									_textBuffer.Append('$');
									break;
								case VelocityLexer.EscapedHash:
									_textBuffer.Append('#');
									break;
								default:
									_textBuffer.Append(token.Text);
									break;
							}
						}
						break;
					case VelocityParser.RULE_whitespace:
						if (ReduceWhitespace)
						{
							if (rule.GetToken(VelocityLexer.Newline, 0) != null)
								_textBuffer.Append('\n');
							else
								_textBuffer.Append(' ');
						}
						else
							_textBuffer.Append(rule.GetText());

						break;
					case VelocityParser.RULE_literal:
						var interval = new Interval(context.start.StartIndex + 3, context.Stop.StopIndex - 2);
						var literal = context.Start.InputStream.GetText(interval);
						_textBuffer.Append(literal);
						break;
					default:
						throw new Exception("Unknown Text Sub Rule" + rule.GetType().Name);
				}
			}
			return Expression.Constant(_textBuffer.ToString());
		}

		public Expression VisitLiteral([NotNull] VelocityParser.LiteralContext context)
		{
			var interval = new Interval(context.start.StartIndex + 3, context.Stop.StopIndex - 2);
			var content = context.Start.InputStream.GetText(interval);
			return Expression.Constant(content);
		}


		public Expression VisitRawText([NotNull] VelocityParser.RawTextContext context)
        {
            _textBuffer.Clear();

            for (int i = 0; i < context.ChildCount; i++)
            {
                var token = ((ITerminalNode)context.GetChild(i)).Symbol;

                switch (token.Type)
                {
                    case VelocityLexer.EscapedDollar:
                        _textBuffer.Append('$');
                        break;
                    case VelocityLexer.EscapedHash:
                        _textBuffer.Append('#');
                        break;
                    default:
                        _textBuffer.Append(token.Text);
                        break;
                }
            }

            return Expression.Constant(_textBuffer.ToString());
        }

        public Expression VisitReference([NotNull] VelocityParser.ReferenceContext context)
        {
            return new ReferenceExpression(
                value: Visit(context.referenceBody()),
                raw: context.GetFullText(),
                isSilent: context.Exclamation() != null,
                isFormal: context.LeftCurley() != null
                );
        }

        public Expression VisitReferenceBody([NotNull] VelocityParser.ReferenceBodyContext context)
        {
            var result = VisitVariable(context.variable());
            var further = context.GetRuleContexts<ParserRuleContext>();

			for (int i = 1; i < further.Length; i++)
			{
				var innerContext = further[i];
				var sourceInfo = GetSourceInfo(innerContext);

                switch (innerContext.RuleIndex)
                {
                    case VelocityParser.RULE_propertyInvocation:
                        var property = (VelocityParser.PropertyInvocationContext)innerContext;
                        var propertyName = property.Identifier().GetText();

                        result = _expressionFactory.Property(result, propertyName, sourceInfo);
                        break;
                    case VelocityParser.RULE_methodInvocation:
                        var method = (VelocityParser.MethodInvocationContext)innerContext;
                        var methodName = method.Identifier().GetText();
                        var methodArgs = VisitMany(method.argument_list().expression());

                        result = _expressionFactory.Method(result, methodName, methodArgs, sourceInfo);
                        break;
                    case VelocityParser.RULE_indexInvocation:
                        var index = (VelocityParser.IndexInvocationContext)innerContext;
                        var indexArgs = VisitMany(index.argument_list().expression());

                        result = _expressionFactory.Index(result, indexArgs, sourceInfo);
                        break;
                    default:
                        throw new InvalidOperationException($"Unrecognised reference body kind - {innerContext.GetType().FullName}");
                }

                if (result == Constants.NullExpression)
                    break;
            }

            return result;
        }

        public Expression VisitVariable([NotNull] VelocityParser.VariableContext context)
        {
            var name = context.Identifier().GetText();
            var sourceInfo = GetSourceInfo(context);
            return _expressionFactory.Variable(name, sourceInfo);
        }

        public Expression VisitIntegerLiteral([NotNull] VelocityParser.IntegerLiteralContext context)
        {
            var value = int.Parse(context.GetText());
            return Expression.Constant(value);
        }

        public Expression VisitFloatingPointLiteral([NotNull] VelocityParser.FloatingPointLiteralContext context)
        {
            var value = float.Parse(context.GetText());
            return Expression.Constant(value);
        }

        public Expression VisitBooleanLiteral([NotNull] VelocityParser.BooleanLiteralContext context)
        {
            switch(context.Boolean.Type)
            {
                case VelocityLexer.True:
                    return Constants.True;
                case VelocityLexer.False:
                    return Constants.False;
                default:
                    throw new InvalidOperationException($"'${context.Boolean}' is not a valid boolean expression");
            }
        }

        public Expression VisitStringLiteral([NotNull] VelocityParser.StringLiteralContext context)
        {
            var interval = new Interval(context.start.StartIndex + 1, context.Stop.StopIndex - 1);
            var unquotedText = context.start.InputStream.GetText(interval).Replace("''", "'");
            return Expression.Constant(unquotedText);
        }

        public Expression VisitInterpolatedStringLiteral([NotNull] VelocityParser.InterpolatedStringLiteralContext context)
        {
            //TODO: Should dictionary strings be handled in the lexer/parser?
            // Yes - would be more efficient.
            // No - these are NVelocity extensions, not standard velocity so don't belong in the parsegrammar.
            var originalInputStream = context.Start.InputStream;

            var stringContentInterval = new Interval(context.Start.StartIndex + 1, context.Stop.StopIndex - 1);
            var unquotedText = originalInputStream.GetText(stringContentInterval);

            if (unquotedText.StartsWith("%{") && unquotedText.EndsWith("}"))
            {
                return new DictionaryStringExpression(unquotedText, InterpolateString);
            }

            //Quote Escaping
            unquotedText = unquotedText.Replace("\"\"", "\"");
            return InterpolateString(unquotedText);
        }

        private InterpolatedStringExpression InterpolateString(string content)
        {
            var interpolatedCharStream = new AntlrInputStream(content);
            var stringTemplate = _parser.ParseTemplate(interpolatedCharStream, "Interpolated String", x => x.template());

            //TODO: This needs tidying up
            var parts = VisitMany(stringTemplate.block().GetRuleContexts<ParserRuleContext>());
            //var block = (BlockExpression)new RenderedBlock(parts).Reduce();
            //return new InterpolatedStringExpression(block.Expressions);

            return new InterpolatedStringExpression(parts);
        }

        public Expression VisitList([NotNull] VelocityParser.ListContext context)
        {
            var elementContexts = context
                .argument_list()
                .expression();

            return new ObjectArrayExpression(GetSourceInfo(context), VisitMany(elementContexts));
        }

        public Expression VisitRange([NotNull] VelocityParser.RangeContext context)
        {
            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));

            return new IntegerRangeExpression(left, right, GetSourceInfo(context));
        }

        public Expression VisitDictionaryExpression([NotNull] VelocityParser.DictionaryExpressionContext context)
        {
            var builder = ImmutableDictionary.CreateBuilder<Expression, Expression>();

            foreach (var entry in context.dictionaryEntry())
            {
                builder.Add(Visit(entry.dictionaryKey()), Visit(entry.expression()));
            }

            return new DictionaryExpression(builder.ToImmutable());
        }

        public Expression VisitDictionaryEntry([NotNull] VelocityParser.DictionaryEntryContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitDictionaryKey([NotNull] VelocityParser.DictionaryKeyContext context)
        {
            var identifier = context.Identifier();
            return identifier != null
                ? Expression.Constant(identifier.GetText())
                : Visit(context.@string());
        }


        private IImmutableList<Expression> VisitMany(IReadOnlyList<ParserRuleContext> contexts)
        {
            if (contexts == null || contexts.Count == 0)
                return ImmutableArray<Expression>.Empty;

            var builder = ImmutableArray.CreateBuilder<Expression>(contexts.Count);

            foreach(var context in contexts)
            {
                var visitedContext = Visit(context);
                if (visitedContext == null)
                    throw new InvalidOperationException("Failed to visit");

                builder.Add(visitedContext);
            }

            return builder.ToImmutable();
        }

        public Expression VisitSetDirective([NotNull] VelocityParser.SetDirectiveContext context)
            => VisitAssignment(context.assignment());

        public Expression VisitAssignment([NotNull] VelocityParser.AssignmentContext context)
        {
            var target = Visit(context.reference());
            var value = Visit(context.expression());
            var sourceInfo = GetSourceInfo(context);

            return _expressionFactory.Assign(target, value, sourceInfo);
        }

        public Expression VisitIfBlock([NotNull] VelocityParser.IfBlockContext context)
        {
            var elseBlock = context.ifElseBlock();
            Expression falseContent = elseBlock == null
                ? Constants.EmptyExpression
                : Visit(elseBlock.block());

            var elseIfBlocks = context.GetRuleContexts<VelocityParser.IfElseifBlockContext>();
            for (int i = elseIfBlocks.Length - 1; i >= 0; i--)
            {
                var elseIf = elseIfBlocks[i];
                var innerCondition = new CoerceToBooleanExpression(Visit(elseIf.expression()));
                var elseIfContent = Visit(elseIf.block());

                falseContent = Expression.IfThenElse(innerCondition, elseIfContent, falseContent);
            }

            var condition = new CoerceToBooleanExpression(Visit(context.expression()));
            var trueContent = Visit(context.block());

            return Expression.IfThenElse(condition, trueContent, falseContent);
        }

        public Expression VisitUnaryExpression([NotNull] VelocityParser.UnaryExpressionContext context)
        {
            var target = Visit(context.expression());
            return Expression.Not(VelocityExpressions.CoerceToBoolean(target));
        }

        public Expression VisitMultiplicativeExpression([NotNull] VelocityParser.MultiplicativeExpressionContext context)
            => VisitMathematicalExpression(context.Operator, context.expression(0), context.expression(1), context);

        public Expression VisitAdditiveExpression([NotNull] VelocityParser.AdditiveExpressionContext context)
            => VisitMathematicalExpression(context.Operator, context.expression(0), context.expression(1), context);

        private Expression VisitMathematicalExpression(IToken operationToken, VelocityParser.ExpressionContext left, VelocityParser.ExpressionContext right, VelocityParser.ExpressionContext context)
        {
			VelocityOperator operation;
            switch (operationToken.Type)
            {
                case VelocityLexer.Plus:
                    operation = VelocityOperator.Add;
                    break;
                case VelocityLexer.Minus:
                    operation = VelocityOperator.Subtract;
                    break;
                case VelocityLexer.Multiply:
                    operation = VelocityOperator.Multiply;
                    break;
                case VelocityLexer.Divide:
                    operation = VelocityOperator.Divide;
                    break;
                case VelocityLexer.Modulo:
                    operation = VelocityOperator.Modulo;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operationToken));
            }

            var visitedLeft = Visit(left);
            var visitedRight = Visit(right);
            var sourceInfo = GetSourceInfo(context);

            return _expressionFactory.Binary(visitedLeft, visitedRight, sourceInfo, operation);
        }

        public Expression VisitRelationalExpression([NotNull] VelocityParser.RelationalExpressionContext context)
            => VisitComparisonExpression(context.Operator, context.expression(0), context.expression(1), context);

        public Expression VisitEqualityExpression([NotNull] VelocityParser.EqualityExpressionContext context)
            => VisitComparisonExpression(context.Operator, context.expression(0), context.expression(1), context);

        private Expression VisitComparisonExpression(IToken operationToken, VelocityParser.ExpressionContext left, VelocityParser.ExpressionContext right, VelocityParser.ExpressionContext context)
        {
			VelocityOperator operation;
            switch (operationToken.Type)
            {
                case VelocityLexer.LessThan:
                    operation = VelocityOperator.LessThan;
                    break;
                case VelocityLexer.GreaterThan:
                    operation = VelocityOperator.GreaterThan;
                    break;
                case VelocityLexer.LessThanOrEqual:
                    operation = VelocityOperator.LessThanOrEqual;
                    break;
                case VelocityLexer.GreaterThanOrEqual:
                    operation = VelocityOperator.GreaterThanOrEqual;
                    break;
                case VelocityLexer.Equal:
                    operation = VelocityOperator.Equal;
                    break;
                case VelocityLexer.NotEqual:
                    operation = VelocityOperator.NotEqual;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operationToken));
            }

            var visitedLeft = Visit(left);
            var visitedRight = Visit(right);
            var sourceInfo = GetSourceInfo(context);

            return _expressionFactory.Binary(visitedLeft, visitedRight,sourceInfo, operation);
        }

        public Expression VisitAndExpression([NotNull] VelocityParser.AndExpressionContext context)
        {
            var left = VelocityExpressions.CoerceToBoolean(Visit(context.expression(0)));
            var right = VelocityExpressions.CoerceToBoolean(Visit(context.expression(1)));

            return Expression.AndAlso(left, right);
        }

        public Expression VisitOrExpression([NotNull] VelocityParser.OrExpressionContext context)
        {
            var left = VelocityExpressions.CoerceToBoolean(Visit(context.expression(0)));
            var right = VelocityExpressions.CoerceToBoolean(Visit(context.expression(1)));

            return Expression.OrElse(left, right);
        }

        public Expression VisitCustomDirective([NotNull] VelocityParser.CustomDirectiveContext context)
        {
            var name = context.DirectiveName().GetText();
            var handler = _customDirectives.SingleOrDefault(x => x.Name == name);

            if (handler == null)
                return new UnrecognisedDirective(name, context.GetFullText());

            var args = VisitMany(context.directiveArguments()?.directiveArgument());

            var body = handler.IsBlockDirective
                ? VisitBlock(context.block())
                : null;

            return handler.Build(args, body);
        }


        public Expression VisitDirectiveArgument([NotNull] VelocityParser.DirectiveArgumentContext context)
        {
            var arg = context.expression();
            return arg == null
                ? VisitDirectiveWord(context.directiveWord())
                : Visit(arg);
        }

        public Expression VisitDirectiveWord([NotNull] VelocityParser.DirectiveWordContext context)
            => new DirectiveWord(context.Identifier().GetText());

        private SourceInfo GetSourceInfo(ParserRuleContext context)
        {
            //N.B. the following assumes that the rule does not span multiple lines.
            return new SourceInfo(context.start.Line,
                context.Start.Column + 1,
                context.Stop.Line,
                context.Stop.Column + context.Stop.Text.Length
                );
        }

        public Expression VisitTerminal(ITerminalNode node)
        {
            throw new InvalidOperationException("Terminal nodes should not be visited directly");
        }



        public Expression VisitIfElseBlock([NotNull] VelocityParser.IfElseBlockContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIfElseifBlock([NotNull] VelocityParser.IfElseifBlockContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitPropertyInvocation([NotNull] VelocityParser.PropertyInvocationContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitMethodInvocation([NotNull] VelocityParser.MethodInvocationContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitArgument_list([NotNull] VelocityParser.Argument_listContext context)
        {
            throw new InvalidOperationException();
        }

        public Expression VisitChildren(IRuleNode node)
        {
            throw new NotImplementedException();
        }

        public Expression VisitErrorNode(IErrorNode node)
        {
            throw new NotImplementedException();
        }

        public Expression VisitParenthesisedExpression([NotNull] VelocityParser.ParenthesisedExpressionContext context)
            => Visit(context.expression());

        public Expression VisitDirectiveArguments([NotNull] VelocityParser.DirectiveArgumentsContext context)
        {
            throw new InvalidOperationException();
        }

        public Expression VisitEnd([NotNull] VelocityParser.EndContext context)
        {
            throw new InvalidOperationException();
        }

        public Expression VisitExpression([NotNull] VelocityParser.ExpressionContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitReferenceExpression([NotNull] VelocityParser.ReferenceExpressionContext context)
            => VisitReference(context.reference());

        public Expression VisitStringExpression([NotNull] VelocityParser.StringExpressionContext context)
            => Visit(context.@string());

        public Expression VisitString([NotNull] VelocityParser.StringContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIndexInvocation([NotNull] VelocityParser.IndexInvocationContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitEndOfLineWhitespace([NotNull] VelocityParser.EndOfLineWhitespaceContext context)
        {
            throw new NotImplementedException();
        }


		public Expression VisitWhitespace([NotNull] VelocityParser.WhitespaceContext context)
		{
			throw new NotImplementedException();
		}
	}
}
