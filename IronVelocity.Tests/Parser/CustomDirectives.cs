using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    public class CustomDirectives : ParserTestBase
    {
        [Test]
        public void ShouldParseSingleLineCustomDirectiveWithNoArguments()
        {
            var input = "#test";

            var result = Parse(input, x => x.custom_directive());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IDENTIFIER()?.GetText(), Is.EqualTo("test"));
            Assert.That(result.block(), Is.Null);
        }

        [TestCase("#custom()")]
        [TestCase("#custom(   )")]
        [TestCase("#custom( 123 456 )")]
        public void ShouldParseSingleLineCustomDirectiveWithArguments(string input)
        {
            var result = Parse(input, x => x.custom_directive());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IDENTIFIER()?.GetText(), Is.EqualTo("custom"));

            Assert.That(result.directive_argument_list(), Is.Not.Null);
            Assert.That(result.block(), Is.Null);
        }

        [TestCase("#multiLine()#end")]
        [TestCase("#multiLine(   )#end")]
        [TestCase("#multiLine( 123 456 )#end")]
        public void ShouldParseMultiLineCustomDirectiveWithNoArguments(string input)
        {
            var result = Parse(input, x => x.custom_directive());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IDENTIFIER()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.directive_argument_list(), Is.Not.Null);
            Assert.That(result.block(), Is.Not.Null);
        }

        [Test]
        public void ShouldParseMultiLineCustomDirectiveWithContent()
        {
            var input = "#multiLine\r\nHello\r\n#end";
            var result = Parse(input, x => x.custom_directive());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IDENTIFIER()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.block(), Is.Not.Null);
        }

        [Test]
        public void ShouldParseMultiLineCustomDirectiveWithArguments()
        {
            var input = "#multiLine( $foo $bar) #end";
            var result = Parse(input, x => x.custom_directive());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IDENTIFIER()?.GetText(), Is.EqualTo("multiLine"));
            Assert.That(result.block(), Is.Not.Null);
        }

        [Test]
        public void ShouldNotParseMultiLineDirectiveWithoutEnd()
        {
            var input = "#multiLine ABC123";
            ParseShouldProduceError(input, x => x.template());
        }

    }
}
