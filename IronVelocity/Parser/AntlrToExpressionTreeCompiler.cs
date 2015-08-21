using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IronVelocity.Compilation.AST;
using Antlr4.Runtime;

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
            return Visit(context.reference_body());
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
                    throw new NotImplementedException();
                var method = innerContext as VelocityParser.Method_invocationContext;
                if (method != null)
                {
                    var name = method.IDENTIFIER().GetText();
                    var args = VisitMany(method.argument_list().argument());
                    result = new MethodInvocationExpression(result, name, args, GetSourceInfo(innerContext));
                }
            }

            return result;
        }

        public override Expression VisitVariable([NotNull] VelocityParser.VariableContext context)
        {
            return new VariableExpression(context.IDENTIFIER().GetText());
        }

        public override Expression VisitMethod_invocation([NotNull] VelocityParser.Method_invocationContext context)
        {
            throw new InvalidOperationException("Method Invocation node should not be visited directly");
        }


        public override Expression VisitArgument_list([NotNull] VelocityParser.Argument_listContext context)
        {
            throw new InvalidOperationException("Arguments list node should not be visited directly");
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
            //TODO: I think this will (incorrectly) capture the quotes in the string
            return Expression.Constant(context.GetText());
        }

        public override Expression VisitInterpolated_string([NotNull] VelocityParser.Interpolated_stringContext context)
        {
            //TODO: I think this will (incorrectly) capture the quotes in the string
            //TODO: need to process the interpolated string
            //TODO: account for dictionary strings
            return Expression.Constant(context.GetText());
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

            var visited = new Expression[contexts.Count];

            for (int i = 0; i < contexts.Count; i++)
            {
                visited[i] = Visit(contexts[i]);
            }

            return visited;

        }

        private SourceInfo GetSourceInfo(ParserRuleContext context)
        {
            //TODO: the stop info is incorrect here
            return new SourceInfo(context.start.Line, context.start.Column, context.stop.Line, context.stop.Column);
        }
    }
}
