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
        [TestCase("[true..[]]", typeof(VelocityParser.BooleanContext), typeof(VelocityParser.ListContext), IgnoreReason = "This currently passes, but should it?" )]
        public void ParseRangeExpression(string input, Type leftArgType, Type rightArgType)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).range();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.argument(0)?.GetChild(0), Is.InstanceOf(leftArgType));
            Assert.That(result.argument(1)?.GetChild(0), Is.InstanceOf(rightArgType));

        }
    }
}
