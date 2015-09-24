using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class ListTests : ParserTestBase
    {
        [TestCase("[]", 0)]
        [TestCase("[ ]", 0)]
        [TestCase("[1]", 1)]
        [TestCase("[ [] ]", 1)]
        [TestCase("[ [false] ]", 1)]
        [TestCase("[$a, 23, 'foo']", 3)]
        public void ParseList(string input, int argumentCount)
        {
            var result = Parse(input, x => x.list(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetFullText(), Is.EqualTo(input));

            var args = result.argument_list();
            Assert.That(args, Is.Not.Null);
            Assert.That(args.argument(), Has.Length.EqualTo(argumentCount));
        }
    }
}
