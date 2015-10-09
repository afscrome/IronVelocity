using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class FloatLiteralTests : ParserTestBase
    {
        [TestCase("1.2")]
        [TestCase("-3.4")]
        [TestCase("0.12")]
        [TestCase("-0.18")]
        [TestCase("12345.67890")]
        [TestCase("-87654.32109")]
        //TODO: Looks like velocity 1.7 allows specifying floats int he form 1.4E12
        public void ParseFloatLiteral(string input)
        {
            var result = Parse(input, x => x.@float(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }

    }
}
