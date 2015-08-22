using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class FloatTests : ParserTestBase
    {
        [TestCase("1.2")]
        [TestCase("-3.4")]
        [TestCase("0.12")]
        [TestCase("-0.18")]
        [TestCase("12345.67890")]
        [TestCase("-87654.32109")]
        [TestCase(".72", Ignore = true, IgnoreReason = "TODO: Not allowed in NVelocity, should it be allowed?")]
        [TestCase("-.1", Ignore = true, IgnoreReason = "TODO: Not allowed in NVelocity, should it be allowed?")]
        //TODO: Looks like velocity 1.7 allows specifying floats int he form 1.4E12
        public void ParseFloatLiteral(string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).@float();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }

    }
}
