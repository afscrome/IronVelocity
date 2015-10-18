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

        [TestCase("#test")]
        [TestCase("#{test}")]
        public void ShouldParseLineCustomDirectiveWithNoArguments(string input)
        {
            var result = Parse(input, x => x.customDirective());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("test"));
            Assert.That(result.block(), Is.Null);
        }

        [TestCase("#custom()")]
        [TestCase("#custom(   )")]
        [TestCase("#custom( 123 456 )")]
        [TestCase("#{custom}()")]
        public void ShouldParseLineCustomDirectiveWithArguments(string input)
        {
            var result = Parse(input, x => x.customDirective());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("custom"));

            Assert.That(result.directiveArguments(), Is.Not.Null);
            Assert.That(result.block(), Is.Null);
        }

        [TestCase("#multiLine()#end")]
        [TestCase("#multiLine(   )#end")]
        [TestCase("#multiLine( 123 456 )#end")]
        [TestCase("#{multiLine}( 123 456 )#end")]
        public void ShouldParseBlockCustomDirectiveWithContent(string input)
        {
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.directiveArguments(), Is.Not.Null);
            Assert.That(result.block(), Is.Not.Null);
        }

        [TestCase("#multiLine\r\nHello\r\n#end")]
        [TestCase("#{multiLine}\r\nHello\r\n#end")]
        public void ShouldParseBlockCustomDirectiveWithNoArgumentsContent(string input)
        {
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.block(), Is.Not.Null);
            Assert.That(result.directiveArguments(), Is.Not.Null);
        }

        [Test]
        public void ShouldParseBlockCustomDirectiveWithArguments()
        {
            var input = "#multiLine( $foo $bar) #end";
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("multiLine"));
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
