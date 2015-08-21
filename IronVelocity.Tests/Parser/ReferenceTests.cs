using IronVelocity.Parser;
using NUnit.Framework;
using System.Linq;

namespace IronVelocity.Tests.Parser
{
    public class ReferenceTests : ParserTestBase
    {
        //TODO: the below tests need to be repeated in both ARGUMENT context, and TEXT lexer states.
        //TODO: Add tests for multiple invocations - e.g. $var.prop.method().prop 
        private readonly int LexerInitialState = VelocityLexer.ARGUMENTS;

        [TestCase("$foo", "foo")]
        [TestCase("$!bar", "bar")]
        [TestCase("${foo}", "foo")]
        [TestCase("$!{bar}", "bar")]
        [TestCase("$CAPITAL", "CAPITAL")]
        [TestCase("$MiXeDCaSe", "MiXeDCaSe")]
        [TestCase("$WithNumbers123", "WithNumbers123")]
        [TestCase("$abcdefghijklmnopqrstuvwxyz_-ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890", "abcdefghijklmnopqrstuvwxyz_-ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890")]
        //TODO: Are ${x-} and ${y_} valid variables (i.e. ending in - or _ )
        public void VariableParseTests(string input, string variableName)
        {
            var reference = CreateParser(input, LexerInitialState).reference();
            Assert.That(reference, Is.Not.Null);
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var variable = reference?.reference_body()?.variable();
            Assert.That(variable, Is.Not.Null);

            Assert.That(variable.IDENTIFIER().GetText(), Is.EqualTo(variableName));
        }

        [TestCase("$foo.dog", "dog")]
        [TestCase("$!bar.cat", "cat")]
        [TestCase("${foo.fish}", "fish")]
        [TestCase("$!{bar.bear}", "bear")]
        public void Property(string input, string propertyName)
        {
            var reference = CreateParser(input, LexerInitialState).reference();
            Assert.That(reference, Is.Not.Null);
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = reference.reference_body()?.property_invocation().Single();
            Assert.That(property, Is.Not.Null);
            Assert.That(property.IDENTIFIER()?.GetText(), Is.EqualTo(propertyName));
        }


        [TestCase("$foo.dog()", "dog")]
        [TestCase("$!bar.cat()", "cat")]
        [TestCase("${foo.fish()}", "fish")]
        [TestCase("$!{bar.bear()}", "bear")]
        public void Method(string input, string methodName)
        {
            var reference = CreateParser(input, LexerInitialState).reference();
            Assert.That(reference, Is.Not.Null);
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var method = reference.reference_body()?.method_invocation().Single();
            Assert.That(method, Is.Not.Null);
            Assert.That(method.IDENTIFIER()?.GetText(), Is.EqualTo(methodName));
            Assert.That(method.argument_list(), Is.Not.Null);
        }


        public void TwoReferenceswithTextInBetween(string reference1, string text, string reference2)
        {
            var input = reference1 + text + reference2;
            var result = CreateParser(input).template();

            var textNode = result.text().Single();
            var references = result.reference();

            Assert.That(textNode.GetText(), Is.EqualTo(text));
            Assert.That(references, Contains.Item(reference1));
            Assert.That(references, Contains.Item(reference2));
        }
    }
}
