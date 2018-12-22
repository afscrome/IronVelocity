using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class RelationalExpressionTests : ParserTestBase
    {
        [TestCase(">",  VelocityLexer.GreaterThan)]
        [TestCase("<",  VelocityLexer.LessThan)]
        [TestCase(">=",  VelocityLexer.GreaterThanOrEqual)]
        [TestCase("<=",  VelocityLexer.LessThanOrEqual)]
        [TestCase("gt",  VelocityLexer.GreaterThan)]
        [TestCase("lt",  VelocityLexer.LessThan)]
        [TestCase("ge",  VelocityLexer.GreaterThanOrEqual)]
        [TestCase("le",  VelocityLexer.LessThanOrEqual)]
        public void ParseBinaryRelationalExpressionTests(string @operator, int operatorTokenKind)
        {
            var left = "$left";
            var right = "$right";
            var input = $"{left} {@operator} {right}";

            ParseBinaryExpressionTest(input, left, right, operatorTokenKind, x => x.expression());
        }
    }
}
