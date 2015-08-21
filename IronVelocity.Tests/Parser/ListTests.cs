using IronVelocity.Parser;
using NUnit.Framework;


namespace IronVelocity.Tests.Parser
{
    public class ListTests : ParserTestBase
    {
        [TestCase("[]")]
        [TestCase("[ ]")]
        [TestCase("[1]")]
        [TestCase("[ [] ]")]
        public void ListParseTest(string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).list();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.arguments(), Is.Not.Null);
        }
    }
}
