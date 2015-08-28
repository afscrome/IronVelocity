using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class BooleanLiteralTests : ParserTestBase
    {
        [TestCase("true")]
        [TestCase("false")]
        public void ParseBooleanLiteral(string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).boolean();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
