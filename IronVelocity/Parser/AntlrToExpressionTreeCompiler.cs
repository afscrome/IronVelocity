using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IronVelocity.Compilation.AST;
using Antlr4.Runtime;
using IronVelocity.Compilation;

namespace IronVelocity.Parser
{
    public class AntlrToExpressionTreeCompiler : VelocityParserBaseVisitor<Expression>
    {
        public override Expression Visit(IParseTree tree)
        {
            return base.Visit(tree);
        }

        public override Expression VisitTemplate([NotNull] VelocityParser.TemplateContext context)
        {
            return Visit(context.block());
        }

        public override Expression VisitBlock([NotNull] VelocityParser.BlockContext context)
        {
            return new RenderedBlock(
                VisitMany(context.GetRuleContexts<ParserRuleContext>())
                );
        }

        public override Expression VisitBlock_comment([NotNull] VelocityParser.Block_commentContext context)
        {
            return Constants.EmptyExpression;
        }

        public override Expression VisitComment([NotNull] VelocityParser.CommentContext context)
        {
            return Constants.EmptyExpression;
        }

        public override Expression VisitText([NotNull] VelocityParser.TextContext context)
        {
            return Expression.Constant(context.GetText());
        }

        public override Expression VisitReference([NotNull] VelocityParser.ReferenceContext context)
        {
            return new ReferenceExpression2(
                value: Visit(context.reference_body()),
                raw: context.GetText(),
                isSilent: context.EXCLAMATION() != null,
                isFormal: context.LEFT_CURLEY() != null
                );
        }

        public override Expression VisitReference_body([NotNull] VelocityParser.Reference_bodyContext context)
        {
            var result = VisitVariable(context.variable());

            var further = context.GetRuleContexts<ParserRuleContext>();

            for (int i = 0; i < further.Count; i++)
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
                        var args = VisitMany(method.argument_list().argument());
                        result = new MethodInvocationExpression(result, name, args, GetSourceInfo(innerContext));
                    }
                }
            }

            return result;
        }

        public override Expression VisitVariable([NotNull] VelocityParser.VariableContext context)
        {
            return new VariableExpression(context.IDENTIFIER().GetText());
        }

        public override Expression VisitArgument([NotNull] VelocityParser.ArgumentContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public override Expression VisitPrimary_expression([NotNull] VelocityParser.Primary_expressionContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public override Expression VisitInteger([NotNull] VelocityParser.IntegerContext context)
        {
            var value = int.Parse(context.GetText());
            return Expression.Constant(value);
        }

        public override Expression VisitFloat([NotNull] VelocityParser.FloatContext context)
        {
            var value = float.Parse(context.GetText());
            return Expression.Constant(value);
        }

        public override Expression VisitBoolean([NotNull] VelocityParser.BooleanContext context)
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

        public override Expression VisitString([NotNull] VelocityParser.StringContext context)
        {
            //HACK: This really should be handled at the parser level, not through a substring operation
            var quotedText = context.GetText();
            var unquotedString = quotedText.Substring(1, quotedText.Length - 2);
            return Expression.Constant(unquotedString);
        }

        public override Expression VisitInterpolated_string([NotNull] VelocityParser.Interpolated_stringContext context)
        {
            //HACK: This really should be handled at the parser level, not through a substring operation
            var quotedText = context.GetText();
            var unquotedString = quotedText.Substring(1, quotedText.Length - 2);
            return Expression.Constant(unquotedString);
        }

        public override Expression VisitList([NotNull] VelocityParser.ListContext context)
        {
            var elementContexts = context
                .argument_list()
                .argument();

            return new ObjectArrayExpression(GetSourceInfo(context), VisitMany(elementContexts));
        }

        public override Expression VisitRange([NotNull] VelocityParser.RangeContext context)
        {
            var left = Visit(context.argument(0));
            var right = Visit(context.argument(1));

            return new IntegerRangeExpression(left, right, GetSourceInfo(context));
        }

        private IReadOnlyList<Expression> VisitMany(IReadOnlyList<ParserRuleContext> contexts)
        {
            if (contexts.Count == 0)
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

        public override Expression VisitSet_directive([NotNull] VelocityParser.Set_directiveContext context)
        {
            return VisitAssignment(context.assignment());
        }

        public override Expression VisitAssignment([NotNull] VelocityParser.AssignmentContext context)
        {
            var left = Visit(context.reference());

            if (left is MethodInvocationExpression)
            {
                //TODO: log?, throw?
                throw new InvalidOperationException("Cannot assign to a method");
            }

            var right = Visit(context.argument());

            return new SetDirective(left, right, GetSourceInfo(context));
        }

        public override Expression VisitIf_block([NotNull] VelocityParser.If_blockContext context)
        {
            var elseBlock = context.if_else_block();
            Expression falseContent = elseBlock == null
                ? Constants.EmptyExpression
                : Visit(elseBlock.block());

            var elseIfBlocks = context.GetRuleContexts<VelocityParser.If_elseif_blockContext>();
            for (int i = elseIfBlocks.Count - 1; i >= 0; i--)
            {
                var elseIf = elseIfBlocks[i];
                var innerCondition = new CoerceToBooleanExpression(Visit(elseIf.argument()));
                var elseIfContent = Visit(elseIf.block());

                falseContent = Expression.IfThenElse(innerCondition, elseIfContent, falseContent);
            }

            var condition = new CoerceToBooleanExpression(Visit(context.argument()));
            var trueContent = Visit(context.block());

            return Expression.IfThenElse(condition, trueContent, falseContent);
        }

        public override Expression VisitOr_expression([NotNull] VelocityParser.Or_expressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.and_expression());

            var left = VelocityExpressions.CoerceToBoolean(Visit(context.or_expression()));
            var right = VelocityExpressions.CoerceToBoolean(Visit(context.and_expression()));

            return Expression.OrElse(left, right);
        }

        public override Expression VisitAnd_expression([NotNull] VelocityParser.And_expressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.primary_expression());

            var left = VelocityExpressions.CoerceToBoolean(Visit(context.and_expression()));
            var right = VelocityExpressions.CoerceToBoolean(Visit(context.primary_expression()));

            return Expression.AndAlso(left, right);
        }

        private SourceInfo GetSourceInfo(ParserRuleContext context)
        {
            //TODO: the stop info is incorrect here
            return new SourceInfo(context.start.Line, context.start.Column, context.stop.Line, context.stop.Column);
        }

        public override Expression VisitTerminal(ITerminalNode node)
        {
            throw new InvalidOperationException("Terminal nodes should not be visited directly");
        }

        protected override Expression AggregateResult(Expression aggregate, Expression nextResult)
        {
            throw new InvalidOperationException("This method should not be called");
        }
    }
}
