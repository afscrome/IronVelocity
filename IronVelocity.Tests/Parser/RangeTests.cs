using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class RangeTests : ParserTestBase
    {
        [TestCase("[1..2]", typeof(VelocityParser.IntegerContext), typeof(VelocityParser.IntegerContext))]
        [TestCase("[$reference..3]", typeof(VelocityParser.ReferenceContext), typeof(VelocityParser.IntegerContext))]
        [TestCase("[-53..$var]", typeof(VelocityParser.IntegerContext), typeof(VelocityParser.ReferenceContext))]
        [TestCase("[$ref..$otherRef]", typeof(VelocityParser.ReferenceContext), typeof(VelocityParser.ReferenceContext))]
        [TestCase("[true..[]]", typeof(VelocityParser.BooleanContext), typeof(VelocityParser.ListContext))]
        public void ParseRangeExpression(string input, Type leftArgType, Type rightArgType)
        {
            var result = Parse(input, x => x.range(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(GetPrimaryExpression(result.expression(0))?.GetChild(0), Is.InstanceOf(leftArgType));
            Assert.That(GetPrimaryExpression(result.expression(1))?.GetChild(0), Is.InstanceOf(rightArgType));

        }

        private VelocityParser.Primary_expressionContext GetPrimaryExpression(VelocityParser.ExpressionContext expression)
        {
            IParseTree tree = expression;

            while (tree != null)
            {
                var primaryExpression = tree as VelocityParser.Primary_expressionContext;
                if (primaryExpression != null)
                    return primaryExpression;

                tree = tree.GetChild(0);
            }

            throw new ArgumentOutOfRangeException(nameof(expression));
        }
    }
}
