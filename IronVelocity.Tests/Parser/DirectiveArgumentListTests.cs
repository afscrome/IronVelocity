using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class DirectiveArgumentListTests : ParserTestBase
    {
        [TestCase(0, "")]
        [TestCase(0, " ")]
        [TestCase(0, "\t")]
        [TestCase(0, "\t \t\t  ")]
        [TestCase(1, "1")]
        [TestCase(1, "  1")]
        [TestCase(1, "1  ")]
        [TestCase(1, "  1   ")]
        [TestCase(2, "1 1 + 1")]
        [TestCase(3, "1'hello'1")]
        [TestCase(3, " 1   1   1 ")]
        [TestCase(3, "$a in $b")]
        public void ParseDirectiveArgumentList(int argumentCount, string input)
        {
            input = $"({input})";
            var result = Parse(input, x => x.directiveArguments(), VelocityLexer.EXPRESSION);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetFullText(), Is.EqualTo(input.Trim()));

            var arguments = result.directiveArgument();
            Assert.That(arguments, Has.Length.EqualTo(argumentCount));
        }

        [TestCase("1,1")]
        public void ParseInvalidDirectiveArgumentList(string input)
        {
            ParseShouldProduceError(input, x => x.directiveArguments(), VelocityLexer.EXPRESSION);
        }
    }
}
