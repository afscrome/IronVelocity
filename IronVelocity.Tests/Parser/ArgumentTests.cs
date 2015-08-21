using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class ArgumentTests : ParserTestBase
    {
        [TestCase("123", typeof(VelocityParser.IntegerContext))]
        [TestCase("7.4", typeof(VelocityParser.FloatContext))]
        [TestCase("true", typeof(VelocityParser.BooleanContext))]
        [TestCase("'string'", typeof(VelocityParser.StringContext))]
        [TestCase("\"interpolated\"", typeof(VelocityParser.Interpolated_stringContext))]
        [TestCase("[ ]", typeof(VelocityParser.ListContext))]
        [TestCase("[1..3]", typeof(VelocityParser.RangeContext))]
        public void ArgumentParseTest(string input, Type parsedNodeType)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).argument();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.GetChild(0), Is.InstanceOf(parsedNodeType));
        }
    }
}
