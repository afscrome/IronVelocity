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
            ParseBinaryExpressionTest(input, left, right, VelocityLexer.Or, x => x.expression());
        }

        [Test]
        public void ParseTernaryOrExpressionTests()
        {
            var input = "$a || $b or $c";
            ParseTernaryExpressionWithEqualPrecedenceTest(input, VelocityLexer.Or, VelocityLexer.Or, x=> x.expression());
        }

    }
}
