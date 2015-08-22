using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class AssignmentTests : ParserTestBase
    {
        [TestCase("$x=123")]
        [TestCase("$x.prop=123")]
        [TestCase("$x.method().prop=123")]
        [TestCase("$x = 123")]
        [TestCase(" $x = 123 ")]
        public void ParseAssignmentExpression(string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).assignment();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.reference(), Is.Not.Null);
            Assert.That(result.argument(), Is.Not.Null);
        }
    }
}
