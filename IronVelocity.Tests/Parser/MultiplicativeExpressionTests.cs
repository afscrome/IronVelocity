using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class Multiplicative : ParserTestBase
    {
        [TestCase("*",  VelocityLexer.MULTIPLY)]
        [TestCase("/", VelocityLexer.DIVIDE)]
        [TestCase("%", VelocityLexer.MODULO)]
        public void ParseBinaryMultiplicitiveExpressionTests(string @operator, int operatorTokenKind)
        {
            var left = "$left";
            var right = "$right";
            var input = $"{left} {@operator} {right}";

            ParseBinaryExpressionTest(input, left, right, operatorTokenKind, x => x.multiplicativeExpression());
        }
    }
}
