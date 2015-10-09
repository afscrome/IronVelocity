using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class ArgumentListTests : ParserTestBase
    {
        [TestCase(0, "")]
        [TestCase(0, " ")]
        [TestCase(0, "\t")]
        [TestCase(0, "\t \t\t  ")]
        [TestCase(1, "1")]
        [TestCase(1, "  1")]
        [TestCase(1,"1  ")]
        [TestCase(1, "  1   ")]
        [TestCase(3, "1,1,1")]
        [TestCase(3, " 1 , 1 , 1 ")]
        public void ParseArgumentList(int argumentCount, string input)
        {
            var result = Parse(input, x => x.argument_list(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetFullText(), Is.EqualTo(input.Trim()));

            var arguments = result.GetRuleContexts<VelocityParser.ExpressionContext>();
            Assert.That(arguments, Has.Length.EqualTo(argumentCount));
        }

        [TestCase(",")]
        [TestCase(",,")]
        [TestCase("1,,1")]
        [TestCase("1,")]
        [TestCase(",1")]
        public void ParseInvalidArgumentList(string input)
        {
            ParseShouldProduceError(input, x => x.argument_list(), VelocityLexer.ARGUMENTS);
        }
    }
}
