using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class StringTests : ParserTestBase
    {
        [TestCase("''")]
        [TestCase("'Hello World'")]
        [TestCase("'\"'")]
        public void ParseStringLiteral(string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).@string();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
