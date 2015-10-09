using System;
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

namespace IronVelocity.Parser
{
    public class AntlrToExpressionTreeCompiler : IVelocityParserVisitor<Expression>
    {
        private readonly AntlrVelocityParser _parser;
        private readonly IReadOnlyCollection<CustomDirectiveBuilder> _customDirectives;

        public AntlrToExpressionTreeCompiler(AntlrVelocityParser parser, IReadOnlyCollection<CustomDirectiveBuilder> customDirectives)
        {
            _parser = parser;
            _customDirectives = customDirectives ?? new CustomDirectiveBuilder[0];
        }

        public Expression Visit(IParseTree tree) => tree.Accept(this);

        public Expression VisitTemplate([NotNull] VelocityParser.TemplateContext context) => Visit(context.block());

        public Expression VisitBlock([NotNull] VelocityParser.BlockContext context)
        {
            return new RenderedBlock(
                VisitMany(context.GetRuleContexts<ParserRuleContext>())
                );
        }

        public Expression VisitBlock_comment([NotNull] VelocityParser.Block_commentContext context) => Constants.EmptyExpression;

        public Expression VisitComment([NotNull] VelocityParser.CommentContext context) => Constants.EmptyExpression;

        private readonly StringBuilder _textBuffer = new StringBuilder();

        public Expression VisitText([NotNull] VelocityParser.TextContext context)
        {
            _textBuffer.Clear();

            for (int i = 0; i < context.ChildCount; i++)
            {
                var token = ((ITerminalNode)context.GetChild(i)).Symbol;

                switch (token.Type)
                {
                    case VelocityLexer.WHITESPACE:
                    case VelocityLexer.NEWLINE:
                        goto default;
                    case VelocityLexer.ESCAPED_DOLLAR:
                        _textBuffer.Append('$');
                        break;
                    case VelocityLexer.ESCAPED_HASH:
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
                value: Visit(context.reference_body()),
                raw: context.GetFullText(),
                isSilent: context.EXCLAMATION() != null,
                isFormal: context.LEFT_CURLEY() != null
                );
        }

        public Expression VisitReference_body([NotNull] VelocityParser.Reference_bodyContext context)
        {
            var result = VisitVariable(context.variable());

            var further = context.GetRuleContexts<ParserRuleContext>();

            for (int i = 0; i < further.Length; i++)
            {
                var innerContext = further[i];
                var property = innerContext as VelocityParser.Property_invocationContext;
                if (property != null)
                {
                    var name = property.IDENTIFIER().GetText();
                    result = new PropertyAccessExpression(result, name, GetSourceInfo(innerContext));
                }
                else
                {
                    var method = innerContext as VelocityParser.Method_invocationContext;
                    if (method != null)
                    {
                        var name = method.IDENTIFIER().GetText();
                        var args = VisitMany(method.argument_list().expression());
                        result = new MethodInvocationExpression(result, name, args, GetSourceInfo(innerContext));
                    }
                }
            }

            return result;
        }

        public Expression VisitVariable([NotNull] VelocityParser.VariableContext context) => new VariableExpression(context.IDENTIFIER().GetText());

        public Expression VisitPrimary_expression([NotNull] VelocityParser.Primary_expressionContext context) => Visit(context.GetRuleContext<ParserRuleContext>(0));

        public Expression VisitInteger([NotNull] VelocityParser.IntegerContext context)
        {
            var value = int.Parse(context.GetText());
            return Expression.Constant(value);
        }

        public Expression VisitFloat([NotNull] VelocityParser.FloatContext context)
        {
            var value = float.Parse(context.GetText());
            return Expression.Constant(value);
        }

        public Expression VisitBoolean([NotNull] VelocityParser.BooleanContext context)
        {
            var text = context.GetText();
            switch (text)
            {
                case "true":
                    return Constants.True;
                case "false":
                    return Constants.False;
                default:
                    throw new InvalidOperationException($"'${text}' is not a valid boolean expression");
            }
        }

        public Expression VisitString([NotNull] VelocityParser.StringContext context)
        {
            var interval = new Interval(context.start.StartIndex + 1, context.Stop.StopIndex - 1);
            var unquotedText = context.start.InputStream.GetText(interval);
            return Expression.Constant(unquotedText);
        }

        public Expression VisitInterpolated_string([NotNull] VelocityParser.Interpolated_stringContext context)
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

        private IReadOnlyList<Expression> VisitMany(IReadOnlyList<ParserRuleContext> contexts)
        {
            if (contexts == null || contexts.Count == 0)
                return new Expression[0]; //TODO: Use Array.Empty

            var visitedExpressions = new Expression[contexts.Count];

            for (int i = 0; i < contexts.Count; i++)
            {
                var visitedContext = Visit(contexts[i]);
                if (visitedContext == null)
                    throw new InvalidOperationException("Failed to visit");

                visitedExpressions[i] = visitedContext;
            }

            return visitedExpressions;
        }

        public Expression VisitSet_directive([NotNull] VelocityParser.Set_directiveContext context)
            => VisitAssignment(context.assignment());

        public Expression VisitAssignment([NotNull] VelocityParser.AssignmentContext context)
        {
            var left = Visit(context.reference());

            if (left is MethodInvocationExpression)
            {
                //TODO: log?, throw?
                throw new InvalidOperationException("Cannot assign to a method");
            }

            var right = Visit(context.expression());

            return new SetDirective(left, right, GetSourceInfo(context));
        }

        public Expression VisitIf_block([NotNull] VelocityParser.If_blockContext context)
        {
            var elseBlock = context.if_else_block();
            Expression falseContent = elseBlock == null
                ? Constants.EmptyExpression
                : Visit(elseBlock.block());

            var elseIfBlocks = context.GetRuleContexts<VelocityParser.If_elseif_blockContext>();
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



        public Expression VisitUnary_expression([NotNull] VelocityParser.Unary_expressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.GetChild(0));

            var target = Visit(context.GetChild(context.ChildCount - 1));
            return Expression.Not(VelocityExpressions.CoerceToBoolean(target));
        }

        public Expression VisitMultiplicative_expression([NotNull] VelocityParser.Multiplicative_expressionContext context)
            => VisitMathematicalExpression(context);

        public Expression VisitAdditive_expression([NotNull] VelocityParser.Additive_expressionContext context)
            => VisitMathematicalExpression(context);

        private Expression VisitMathematicalExpression(ParserRuleContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.GetChild(0));

            if (context.ChildCount != 3)
                throw new ArgumentOutOfRangeException(nameof(context));

            var operatorKind = ((ITerminalNode)context.GetChild(1)).Symbol.Type;
            MathematicalOperation operation;
            switch (operatorKind)
            {
                case VelocityLexer.PLUS:
                    operation = MathematicalOperation.Add;
                    break;
                case VelocityLexer.MINUS:
                    operation = MathematicalOperation.Subtract;
                    break;
                case VelocityLexer.MULTIPLY:
                    operation = MathematicalOperation.Multiply;
                    break;
                case VelocityLexer.DIVIDE:
                    operation = MathematicalOperation.Divide;
                    break;
                case VelocityLexer.MODULO:
                    operation = MathematicalOperation.Modulo;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context));
            }

            var left = Visit(context.GetChild(0));
            var right = Visit(context.GetChild(2));
            var sourceInfo = GetSourceInfo(context);

            return new MathematicalExpression(left, right, sourceInfo, operation);
        }

        public Expression VisitRelational_expression([NotNull] VelocityParser.Relational_expressionContext context)
            => VisitComparisonExpression(context);

        public Expression VisitEquality_expression([NotNull] VelocityParser.Equality_expressionContext context)
            => VisitComparisonExpression(context);

        private Expression VisitComparisonExpression(ParserRuleContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.GetChild(0));

            if (context.ChildCount != 3)
                throw new ArgumentOutOfRangeException(nameof(context));

            var operatorKind = ((ITerminalNode)context.GetChild(1)).Symbol.Type;

            ComparisonOperation operation;
            switch (operatorKind)
            {
                case VelocityLexer.LESSTHAN:
                    operation = ComparisonOperation.LessThan;
                    break;
                case VelocityLexer.GREATERTHAN:
                    operation = ComparisonOperation.GreaterThan;
                    break;
                case VelocityLexer.LESSTHANOREQUAL:
                    operation = ComparisonOperation.LessThanOrEqual;
                    break;
                case VelocityLexer.GREATERTHANOREQUAL:
                    operation = ComparisonOperation.GreaterThanOrEqual;
                    break;
                case VelocityLexer.EQUAL:
                    operation = ComparisonOperation.Equal;
                    break;
                case VelocityLexer.NOTEQUAL:
                    operation = ComparisonOperation.NotEqual;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context));
            }

            var left = Visit(context.GetChild(0));
            var right = Visit(context.GetChild(2));
            var sourceInfo = GetSourceInfo(context);

            return new ComparisonExpression(left, right, sourceInfo, operation);
        }

        public Expression VisitExpression([NotNull] VelocityParser.ExpressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.and_expression());

            var left = VelocityExpressions.CoerceToBoolean(Visit(context.expression()));
            var right = VelocityExpressions.CoerceToBoolean(Visit(context.and_expression()));

            return Expression.OrElse(left, right);
        }

        public Expression VisitAnd_expression([NotNull] VelocityParser.And_expressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.equality_expression());

            var left = VelocityExpressions.CoerceToBoolean(Visit(context.and_expression()));
            var right = VelocityExpressions.CoerceToBoolean(Visit(context.equality_expression()));

            return Expression.AndAlso(left, right);
        }

        public Expression VisitCustom_directive([NotNull] VelocityParser.Custom_directiveContext context)
        {
            var name = context.DIRECTIVE_NAME().GetText();
            var handler = _customDirectives.SingleOrDefault(x => x.Name == name);

            if (handler == null)
                return new UnrecognisedDirective(name, context.GetFullText());

            var args = VisitMany(context.directive_arguments()?.directive_argument());

            var body = handler.IsBlockDirective
                ? VisitBlock(context.block())
                : null;

            return handler.Build(args, body);
       }


        public Expression VisitDirective_argument([NotNull] VelocityParser.Directive_argumentContext context)
        {
            var arg = context.expression();
            return arg == null
                ? VisitDirective_word(context.directive_word())
                : VisitExpression(arg);
        }

        public Expression VisitDirective_word([NotNull] VelocityParser.Directive_wordContext context)
            => new DirectiveWord(context.IDENTIFIER().GetText());

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



        public Expression VisitIf_else_block([NotNull] VelocityParser.If_else_blockContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIf_elseif_block([NotNull] VelocityParser.If_elseif_blockContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitProperty_invocation([NotNull] VelocityParser.Property_invocationContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitMethod_invocation([NotNull] VelocityParser.Method_invocationContext context)
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

        public Expression VisitParenthesised_expression([NotNull] VelocityParser.Parenthesised_expressionContext context)
            => Visit(context.expression());

        public Expression VisitDirective_arguments([NotNull] VelocityParser.Directive_argumentsContext context)
        {
            throw new InvalidOperationException();
        }

        public Expression VisitEnd([NotNull] VelocityParser.EndContext context)
        {
            throw new InvalidOperationException();
        }
    }
}
