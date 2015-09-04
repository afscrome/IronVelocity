using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IronVelocity.Compilation.AST;
using Antlr4.Runtime;
using IronVelocity.Compilation;
using IronVelocity.Binders;

namespace IronVelocity.Parser
{
    public class AntlrToExpressionTreeCompiler : IVelocityParserVisitor<Expression>
    {
        public Expression Visit(IParseTree tree)
        {
            return tree.Accept(this);
        }

        public Expression VisitTemplate([NotNull] VelocityParser.TemplateContext context)
        {
            return Visit(context.block());
        }

        public Expression VisitBlock([NotNull] VelocityParser.BlockContext context)
        {
            return new RenderedBlock(
                VisitMany(context.GetRuleContexts<ParserRuleContext>())
                );
        }

        public Expression VisitBlock_comment([NotNull] VelocityParser.Block_commentContext context)
        {
            return Constants.EmptyExpression;
        }

        public Expression VisitComment([NotNull] VelocityParser.CommentContext context)
        {
            return Constants.EmptyExpression;
        }

        public Expression VisitText([NotNull] VelocityParser.TextContext context)
        {
            return Expression.Constant(context.GetText());
        }

        public Expression VisitReference([NotNull] VelocityParser.ReferenceContext context)
        {
            return new ReferenceExpression2(
                value: Visit(context.reference_body()),
                raw: context.GetText(),
                isSilent: context.EXCLAMATION() != null,
                isFormal: context.LEFT_CURLEY() != null
                );
        }

        public Expression VisitReference_body([NotNull] VelocityParser.Reference_bodyContext context)
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

        public Expression VisitVariable([NotNull] VelocityParser.VariableContext context)
        {
            return new VariableExpression(context.IDENTIFIER().GetText());
        }

        public Expression VisitArgument([NotNull] VelocityParser.ArgumentContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public Expression VisitPrimary_expression([NotNull] VelocityParser.Primary_expressionContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

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
            //HACK: This really should be handled at the parser level, not through a substring operation
            var quotedText = context.GetText();
            var unquotedString = quotedText.Substring(1, quotedText.Length - 2);
            return Expression.Constant(unquotedString);
        }

        public Expression VisitInterpolated_string([NotNull] VelocityParser.Interpolated_stringContext context)
        {
            //HACK: This really should be handled at the parser level, not through a substring operation
            var quotedText = context.GetText();
            var unquotedString = quotedText.Substring(1, quotedText.Length - 2);
            return Expression.Constant(unquotedString);
        }

        public Expression VisitList([NotNull] VelocityParser.ListContext context)
        {
            var elementContexts = context
                .argument_list()
                .argument();

            return new ObjectArrayExpression(GetSourceInfo(context), VisitMany(elementContexts));
        }

        public Expression VisitRange([NotNull] VelocityParser.RangeContext context)
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

        public Expression VisitSet_directive([NotNull] VelocityParser.Set_directiveContext context)
        {
            return VisitAssignment(context.assignment());
        }

        public Expression VisitAssignment([NotNull] VelocityParser.AssignmentContext context)
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

        public Expression VisitIf_block([NotNull] VelocityParser.If_blockContext context)
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



        public Expression VisitUnary_expression([NotNull] VelocityParser.Unary_expressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.GetChild(0));

            var target = Visit(context.GetChild(1));
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

        public Expression VisitOr_expression([NotNull] VelocityParser.Or_expressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.and_expression());

            var left = VelocityExpressions.CoerceToBoolean(Visit(context.or_expression()));
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

        private SourceInfo GetSourceInfo(ParserRuleContext context)
        {
            //TODO: the stop info is incorrect here
            return new SourceInfo(context.start.Line, context.start.Column, context.stop.Line, context.stop.Column);
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
    }
}
