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
        public void ParseList(string input)
        {
            var result = Parse(input, x => x.list(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetFullText(), Is.EqualTo(input));

            Assert.That(result.argument_list(), Is.Not.Null);
        }
    }
}
