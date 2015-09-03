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
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).equality_expression();
            ParseBinaryExpressionTest(result, input, left, right, operatorTokenKind);
        }

        [Test]
        public void ParseTernaryEqualityExpressionTests()
        {
            var input = "$a == $b ne $c";
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).equality_expression();
            ParseTernaryExpressionWithEqualPrecedenceTest(result, input, VelocityLexer.EQUAL, VelocityLexer.NOTEQUAL);
        }
    }
}
