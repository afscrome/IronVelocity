using Antlr4.Runtime.Tree;
using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class RangeTests : ParserTestBase
    {
        [TestCase("[1..2]", typeof(VelocityParser.IntegerLiteralContext), typeof(VelocityParser.IntegerLiteralContext))]
        [TestCase("[$reference..3]", typeof(VelocityParser.Reference2Context), typeof(VelocityParser.IntegerLiteralContext))]
        [TestCase("[-53..$var]", typeof(VelocityParser.IntegerLiteralContext), typeof(VelocityParser.Reference2Context))]
        [TestCase("[$ref..$otherRef]", typeof(VelocityParser.Reference2Context), typeof(VelocityParser.Reference2Context))]
        [TestCase("[true..[]]", typeof(VelocityParser.BooleanLiteralContext), typeof(VelocityParser.ListContext))]
        public void ParseRangeExpression(string input, Type leftArgType, Type rightArgType)
        {
            var result = (VelocityParser.RangeContext)Parse(input, x => x.primaryExpression(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.expression(0).GetChild(0), Is.InstanceOf(leftArgType));
            Assert.That(result.expression(1).GetChild(0), Is.InstanceOf(rightArgType));

        }

        private VelocityParser.PrimaryExpressionContext GetPrimaryExpression(VelocityParser.ExpressionContext expression)
        {
            IParseTree tree = expression;

            while (tree != null)
            {
                var primaryExpression = tree as VelocityParser.PrimaryExpressionContext;
                if (primaryExpression != null)
                    return primaryExpression;

                tree = tree.GetChild(0);
            }

            throw new ArgumentOutOfRangeException(nameof(expression));
        }
    }
}
