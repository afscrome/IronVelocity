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
            ParseBinaryExpressionTest(result, input, left, right, VelocityLexer.AND);
        }

        [Test]
        public void ParseTernaryAndExpressionTests()
        {
            var input = "$a && $b and $c";
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).and_expression();

            ParseTernaryExpressionWithEqualPrecedenceTest(result, input, VelocityLexer.AND, VelocityLexer.AND);
        }
    }
}
