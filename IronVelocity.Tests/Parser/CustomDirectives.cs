using NUnit.Framework;
using IronVelocity.Parser;

namespace IronVelocity.Tests.Parser
{
    public class CustomDirectives : ParserTestBase
    {

        private VelocityParser.CustomDirectiveContext ParseBlockDirective(VelocityParser parser)
        {
            parser.BlockDirectives = new[] { "multiLine" };
            return parser.customDirective();
        } 

        [Test]
        public void ShouldParseLineCustomDirectiveWithNoArguments()
        {
            var input = "#test";

            var result = Parse(input, x => x.customDirective());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DIRECTIVE_NAME()?.GetText(), Is.EqualTo("test"));
            Assert.That(result.block(), Is.Null);
        }

        [TestCase("#custom()")]
        [TestCase("#custom(   )")]
        [TestCase("#custom( 123 456 )")]
        public void ShouldParseLineCustomDirectiveWithArguments(string input)
        {
            var result = Parse(input, x => x.customDirective());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DIRECTIVE_NAME()?.GetText(), Is.EqualTo("custom"));

            Assert.That(result.directiveArguments(), Is.Not.Null);
            Assert.That(result.block(), Is.Null);
        }

        [TestCase("#multiLine()#end")]
        [TestCase("#multiLine(   )#end")]
        [TestCase("#multiLine( 123 456 )#end")]
        public void ShouldParseBlockCustomDirectiveWithNoArguments(string input)
        {
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DIRECTIVE_NAME()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.directiveArguments(), Is.Not.Null);
            Assert.That(result.block(), Is.Not.Null);
        }

        [Test]
        public void ShouldParseBlockCustomDirectiveWithContent()
        {
            var input = "#multiLine\r\nHello\r\n#end";
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DIRECTIVE_NAME()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.block(), Is.Not.Null);
            Assert.That(result.directiveArguments(), Is.Not.Null);
        }

        [Test]
        public void ShouldParseBlockCustomDirectiveWithArguments()
        {
            var input = "#multiLine( $foo $bar) #end";
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DIRECTIVE_NAME()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.block(), Is.Not.Null);
            Assert.That(result.directiveArguments(), Is.Not.Null);
        }

        [Test]
        public void ShouldNotParseBlockDirectiveWithoutEnd()
        {
            var input = "#multiLine ABC123";
            ParseShouldProduceError(input, x => {
                x.BlockDirectives = new[] { "multiLine" };
                return x.template();
            });
        }

    }
}
