using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class PrimaryExpressionTests : ParserTestBase
    {
        [TestCase("123", typeof(VelocityParser.IntegerContext))]
        [TestCase("7.4", typeof(VelocityParser.FloatContext))]
        [TestCase("true", typeof(VelocityParser.BooleanContext))]
        [TestCase("'string'", typeof(VelocityParser.StringContext))]
        [TestCase("\"interpolated\"", typeof(VelocityParser.InterpolatedStringContext))]
        [TestCase("[]", typeof(VelocityParser.ListContext))]
        [TestCase("[1..3]", typeof(VelocityParser.RangeContext))]
        [TestCase("(1+2)", typeof(VelocityParser.ParenthesisedExpressionContext))]
        public void ParsePrimaryExpression(string input, Type parsedNodeType)
        {
            var result = Parse(input, x => x.primaryExpression(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.GetChild(0), Is.InstanceOf(parsedNodeType));
        }
    }
}
