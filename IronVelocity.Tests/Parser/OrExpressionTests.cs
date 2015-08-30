using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class OrExpressionTests : ParserTestBase
    {
        [TestCase("$x||$y ", "$x", "$y")]
        [TestCase(" false || true ", "false", "true")]
        [TestCase("${x}or${y} ", "${x}", "${y}")]
        [TestCase(" false or true ", "false", "true")]
        public void ParseBinaryOrExpressionTests(string input, string left, string right)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).or_expression();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.or_expression()?.GetText()?.Trim(), Is.EqualTo(left));
            Assert.That(result.and_expression()?.GetText()?.Trim(), Is.EqualTo(right));
        }

        [Test]
        public void ParseTernaryOrExpressionTests()
        {
            var input = "$a || $b || $c";
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).or_expression();

            Assert.That(result, Is.Not.Null);

            Assert.That(result.or_expression()?.or_expression()?.GetText().Trim(), Is.EqualTo("$a"));
            Assert.That(result.or_expression()?.and_expression()?.GetText().Trim(), Is.EqualTo("$b"));
            Assert.That(result.and_expression()?.GetText().Trim(), Is.EqualTo("$c"));
        }

    }
}
