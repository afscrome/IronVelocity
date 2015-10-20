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
            var result = Parse(input, x => x.expression(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.InstanceOf<VelocityParser.StringLiteralContext>());
            Assert.That(result.GetText(), Is.EqualTo(input));
        }

        [Test]
        public void ParseStringLiteralWithEscapedQuotes()
        {
            var input = "'Jim''s Foo'";

            var result = Parse(input, x => x.expression(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.InstanceOf<VelocityParser.StringLiteralContext>());
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}
