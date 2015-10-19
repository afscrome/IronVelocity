using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class InterpolatedStringTests : ParserTestBase
    {
        [TestCase("\"\"")]
        [TestCase("\"Hello World\"")]
        [TestCase("\"'\"")]
        public void ParseInterpolatedString(string input)
        {
            var result = Parse(input, x => x.expression(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.InstanceOf<VelocityParser.InterpolatedStringLiteralContext>());
            Assert.That(result.GetText(), Is.EqualTo(input));
        }

        [Test]
        public void ParseInterpolatedStringWithEscapedQuotes()
        {
            var input = "\"Bob said \"\"Hello\"\" to his neighbour\"";

            var result = Parse(input, x => x.expression(), VelocityLexer.ARGUMENTS);
            Assert.That(result, Is.InstanceOf<VelocityParser.InterpolatedStringLiteralContext>());
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
