using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class InterpolatedStringTests : ParserTestBase
    {
        [TestCase("\"\"")]
        [TestCase("\"Hello World\"")]
        [TestCase("\"'\"")]
        public void ParseInterpolatedStringLiteral(string input)
        {
            var result = Parse(input, x => x.interpolated_string(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
