using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class OrExpressionTests : ParserTestBase
    {
        [TestCase("$x||$y", "$x", "$y")]
        [TestCase(" false || true ", "false", "true")]
        [TestCase("${x}or${y} ", "${x}", "${y}")]
        [TestCase(" false or true ", "false", "true")]
        public void ParseBinaryOrExpressionTests(string input, string left, string right)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).or_expression();
            ParseBinaryExpressionTest(result, input, left, right, VelocityLexer.OR);
        }

        [Test]
        public void ParseTernaryOrExpressionTests()
        {
            var input = "$a || $b or $c";
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).or_expression();
            ParseTernaryExpressionWithEqualPrecedenceTest(result, input, VelocityLexer.OR, VelocityLexer.OR);
        }

    }
}
