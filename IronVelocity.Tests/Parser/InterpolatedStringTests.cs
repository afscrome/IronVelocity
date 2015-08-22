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
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).interpolated_string();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
