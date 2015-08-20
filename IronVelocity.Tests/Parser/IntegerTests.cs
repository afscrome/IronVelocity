using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class IntegerTests : ParserTestBase
    {
        [TestCase("72")]
        [TestCase("-14")]
        [TestCase("0")]
        [TestCase("04")]
        [TestCase("1234567890")]
        [TestCase("-8765432109")]
        public void IntegerParseTests(string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).integer();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
