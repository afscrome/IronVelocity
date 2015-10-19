using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class IntegerLiteralTests : ParserTestBase
    {
        [TestCase("72")]
        [TestCase("-14")]
        [TestCase("0")]
        [TestCase("04")]
        [TestCase("1234567890")]
        [TestCase("-8765432109")]
        public void ParseIntegerLiteral(string input)
        {
            var result = Parse(input, x => x.expression(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.InstanceOf<VelocityParser.IntegerLiteralContext>());
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
