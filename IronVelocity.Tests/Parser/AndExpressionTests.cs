using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class AndExpressionTests : ParserTestBase
    {
        [TestCase("$x&&$y", "$x", "$y")]
        [TestCase(" false && true ", "false", "true")]
        [TestCase("${x}and${y} ", "${x}", "${y}")]
        [TestCase(" false and true ", "false", "true")]
        public void ParseBinaryAndExpressionTests(string input, string left, string right)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).and_expression();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.and_expression()?.GetText()?.Trim(), Is.EqualTo(left));
            Assert.That(result.equality_expression()?.GetText()?.Trim(), Is.EqualTo(right));
        }

        [Test]
        public void ParseTernaryAndExpressionTests()
        {
            var input = "$a && $b && $c";
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).and_expression();

            Assert.That(result, Is.Not.Null);

            Assert.That(result.and_expression()?.and_expression()?.GetText().Trim(), Is.EqualTo("$a"));
            Assert.That(result.and_expression()?.equality_expression()?.GetText().Trim(), Is.EqualTo("$b"));
            Assert.That(result.equality_expression()?.GetText().Trim(), Is.EqualTo("$c"));
        }
    }
}
