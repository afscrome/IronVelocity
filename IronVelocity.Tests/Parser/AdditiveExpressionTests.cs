using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class AdditiveExpressionTests : ParserTestBase
    {
        [TestCase("+",  VelocityLexer.PLUS)]
        [TestCase("-",  VelocityLexer.MINUS)]
        public void ParseBinaryAdditiveExpressionTests(string @operator, int operatorTokenKind)
        {
            var left = "$left";
            var right = "$right";
            var input = $"{left} {@operator} {right}";

            ParseBinaryExpressionTest(input, left, right, operatorTokenKind, x => x.additive_expression());
        }
    }
}
