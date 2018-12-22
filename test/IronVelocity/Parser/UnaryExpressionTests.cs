using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class UnaryExpressionTests : ParserTestBase
    {
        [TestCase("!",  VelocityLexer.Exclamation)]
        public void ParseUnaryExpressionTests(string @operator, int operatorTokenKind)
        {
            var target= "$target";
            var input = @operator + target;

            var parsed = Parse(input, x => x.expression(), VelocityLexer.EXPRESSION);
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.GetText(), Is.EqualTo(input));
            Assert.That(parsed.ChildCount, Is.EqualTo(2));

            Assert.That(GetTerminalNodeTokenType(parsed.GetChild(0)), Is.EqualTo(operatorTokenKind));

            Assert.That(parsed.GetChild(1).GetText(), Is.EqualTo(target));
        }
    }
}
