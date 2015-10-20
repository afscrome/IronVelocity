using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class EqualityExpressionTests : ParserTestBase
    {
        [TestCase("$x==$y", "$x", "$y", VelocityLexer.Equal)]
        [TestCase(" false != true ", "false", "true", VelocityLexer.NotEqual)]
        [TestCase(" false eq true ", "false", "true", VelocityLexer.Equal)]
        [TestCase("${x}ne${y} ", "${x}", "${y}", VelocityLexer.NotEqual)]
        public void ParseBinaryEqualityExpressionTests(string input, string left, string right, int operatorTokenKind)
        {
            ParseBinaryExpressionTest(input, left, right, operatorTokenKind, x => x.expression());
        }

        [Test]
        public void ParseTernaryEqualityExpressionTests()
        {
            var input = "$a == $b ne $c";
            ParseTernaryExpressionWithEqualPrecedenceTest(input, VelocityLexer.Equal, VelocityLexer.NotEqual, x => x.expression());
        }
    }
}
