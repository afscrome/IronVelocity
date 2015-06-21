using IronVelocity.Parser.AST;
using NUnit.Framework;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class WordTests
    {
        [TestCase("in")]
        [TestCase("TRUE")]
        [TestCase("FALSE")]
        public void Word(string name)
        {
            var parser = new VelocityParserWithStatistics(name);
            var result = parser.Expression();

            Assert.That(parser.BooleanLiteralOrWordCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile);

            Assert.That(result, Is.TypeOf<WordNode>());
            var node = (WordNode)result;
            Assert.That(node.Name, Is.EqualTo(name));
        }

    }
}
