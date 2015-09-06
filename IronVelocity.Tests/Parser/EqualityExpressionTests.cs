using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class EqualityExpressionTests : ParserTestBase
    {
        [TestCase("$x==$y", "$x", "$y", VelocityLexer.EQUAL)]
        [TestCase(" false != true ", "false", "true", VelocityLexer.NOTEQUAL)]
        [TestCase(" false eq true ", "false", "true", VelocityLexer.EQUAL)]
        [TestCase("${x}ne${y} ", "${x}", "${y}", VelocityLexer.NOTEQUAL)]
        public void ParseBinaryEqualityExpressionTests(string input, string left, string right, int operatorTokenKind)
        {
            ParseBinaryExpressionTest(input, left, right, operatorTokenKind, x => x.equality_expression());
        }

        [Test]
        public void ParseTernaryEqualityExpressionTests()
        {
            var input = "$a == $b ne $c";
            ParseTernaryExpressionWithEqualPrecedenceTest(input, VelocityLexer.EQUAL, VelocityLexer.NOTEQUAL, x => x.equality_expression());
        }
    }
}
