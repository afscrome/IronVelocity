using NUnit.Framework;
using IronVelocity.Parser;
using System.Collections.Immutable;
using IronVelocity.Directives;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Tests.Parser
{
    public class CustomDirectives : ParserTestBase
    {
        private VelocityParser.CustomDirectiveContext ParseBlockDirective(VelocityParser parser)
        {
			parser.DirectiveBuilders = new CustomDirectiveBuilder[] {
					new TestDirectiveBuilder("test", false),
					new TestDirectiveBuilder("custom", false),
					new TestDirectiveBuilder("multiLine", true),
				}.ToImmutableList();
            return parser.customDirective();
        }

        [TestCase("#test")]
        [TestCase("#{test}")]
        public void ShouldParseLineCustomDirectiveWithNoArguments(string input)
        {
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("test"));

			var directiveBody = result.directiveBody();
			Assert.That(directiveBody, Is.Not.Null);
			Assert.That(directiveBody.block(), Is.Null);
        }

        [TestCase("#custom()")]
        [TestCase("#custom(   )")]
        [TestCase("#custom( 123 456 )")]
        [TestCase("#{custom}()")]
        public void ShouldParseLineCustomDirectiveWithArguments(string input)
        {
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("custom"));

			var directiveBody = result.directiveBody();
			Assert.That(directiveBody, Is.Not.Null);
			Assert.That(directiveBody.directiveArguments(), Is.Not.Null);
            Assert.That(directiveBody.block(), Is.Null);
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

			var directiveBody = result.directiveBody();
			Assert.That(directiveBody, Is.Not.Null);
			Assert.That(directiveBody.directiveArguments(), Is.Not.Null);
            Assert.That(directiveBody.block(), Is.Not.Null);
        }

        [TestCase("#multiLine\r\nHello\r\n#end")]
        [TestCase("#{multiLine}\r\nHello\r\n#end")]
        public void ShouldParseBlockCustomDirectiveWithNoArgumentsContent(string input)
        {
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("multiLine"));

			var directiveBody = result.directiveBody();
			Assert.That(directiveBody, Is.Not.Null);
			Assert.That(directiveBody.block(), Is.Not.Null);
			Assert.That(directiveBody.directiveArguments(), Is.Not.Null);
        }

        [Test]
        public void ShouldParseBlockCustomDirectiveWithArguments()
        {
            var input = "#multiLine( $foo $bar) #end";
            var result = Parse(input, ParseBlockDirective);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DirectiveName()?.GetText(), Is.EqualTo("multiLine"));


			var directiveBody = result.directiveBody();
			Assert.That(directiveBody, Is.Not.Null);
            Assert.That(directiveBody.block(), Is.Not.Null);
            Assert.That(directiveBody.directiveArguments(), Is.Not.Null);
        }

        [Test]
        public void ShouldNotParseBlockDirectiveWithoutEnd()
        {
            var input = "#multiLine ABC123";
            ParseShouldProduceError(input, ParseBlockDirective);
        }


		private class TestDirectiveBuilder : CustomDirectiveBuilder
		{
			public override bool IsBlockDirective { get; }
			public override string Name { get; }

			public TestDirectiveBuilder(string name, bool isBlock)
			{
				Name = name;
				IsBlockDirective = isBlock;
			}


			public override Expression Build(IImmutableList<Expression> arguments, Expression body) => null;
		}

	}
}
