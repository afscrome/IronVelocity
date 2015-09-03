using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class EqualityExpressionTests : ParserTestBase
    {
        [TestCase("$x==$y", "$x", "==", "$y")]
        [TestCase(" false != true ", "false", "!=", "true")]
        [TestCase(" false eq true ", "false", "eq", "true")]
        [TestCase("${x}ne${y} ", "${x}", "ne", "${y}")]
        public void ParseBinaryEqualityExpressionTests(string input, string left, string op, string right)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).equality_expression();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
            Assert.That(result.ChildCount, Is.EqualTo(3));

            Assert.That(result.GetChild(0).GetText().Trim(), Is.EqualTo(left));
            Assert.That(result.GetChild(1).GetText().Trim(), Is.EqualTo(op));
            Assert.That(result.GetChild(2).GetText().Trim(), Is.EqualTo(right));
        }

        [Test]
        public void ParseTernaryEqualityExpressionTests()
        {
            var input = "$a == $b != $c";
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).equality_expression();

            Assert.That(result, Is.Not.Null);

            Assert.That(result.GetText(), Is.EqualTo(input));
            Assert.That(result.ChildCount, Is.EqualTo(3));
            Assert.That(result.GetChild(0).ChildCount, Is.EqualTo(3));

            Assert.That(result.GetChild(0).GetChild(0).GetText().Trim(), Is.EqualTo("$a"));
            Assert.That(result.GetChild(0).GetChild(1).GetText(), Is.EqualTo("=="));
            Assert.That(result.GetChild(0).GetChild(2).GetText().Trim(), Is.EqualTo("$b"));
            Assert.That(result.GetChild(1).GetText(), Is.EqualTo("!="));
            Assert.That(result.GetChild(2).GetText().Trim(), Is.EqualTo("$c"));
        }
    }
}
