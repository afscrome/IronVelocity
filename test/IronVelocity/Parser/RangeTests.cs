using Antlr4.Runtime.Tree;
using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class RangeTests : ParserTestBase
    {
        [TestCase("[1..2]", typeof(VelocityParser.IntegerLiteralContext), typeof(VelocityParser.IntegerLiteralContext))]
        [TestCase("[$reference..3]", typeof(VelocityParser.ReferenceExpressionContext), typeof(VelocityParser.IntegerLiteralContext))]
        [TestCase("[-53..$var]", typeof(VelocityParser.IntegerLiteralContext), typeof(VelocityParser.ReferenceExpressionContext))]
        [TestCase("[$ref..$otherRef]", typeof(VelocityParser.ReferenceExpressionContext), typeof(VelocityParser.ReferenceExpressionContext))]
        [TestCase("[true..[]]", typeof(VelocityParser.BooleanLiteralContext), typeof(VelocityParser.ListContext))]
        public void ParseRangeExpression(string input, Type leftArgType, Type rightArgType)
        {
            var result = (VelocityParser.RangeContext)Parse(input, x => x.expression(), VelocityLexer.EXPRESSION);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.expression(0), Is.InstanceOf(leftArgType));
            Assert.That(result.expression(1), Is.InstanceOf(rightArgType));

        }
    }
}
