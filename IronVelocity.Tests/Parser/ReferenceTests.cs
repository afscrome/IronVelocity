using IronVelocity.Parser;
using NUnit.Framework;
using System.Linq;

namespace IronVelocity.Tests.Parser
{
    [TestFixture(VelocityLexer.DefaultMode)]
    [TestFixture(VelocityLexer.HASH_SEEN)]
    [TestFixture(VelocityLexer.DOLLAR_SEEN)]
    [TestFixture(VelocityLexer.REFERENCE)]
    [TestFixture(VelocityLexer.REFERENCE_MEMBER_ACCESS)]
    [TestFixture(VelocityLexer.ARGUMENTS)]
    public class ReferenceTests : ParserTestBase
    {
        private readonly int LexerInitialState;

        public ReferenceTests(int initialLexerState)
        {
            LexerInitialState = initialLexerState;
        }

        [TestCase("$foo", "foo")]
        [TestCase("$!bar", "bar")]
        [TestCase("${foo}", "foo")]
        [TestCase("$!{bar}", "bar")]
        [TestCase("$CAPITAL", "CAPITAL")]
        [TestCase("$MiXeDCaSe", "MiXeDCaSe")]
        [TestCase("$WithNumbers123", "WithNumbers123")]
        [TestCase("$abcdefghijklmnopqrstuvwxyz_-ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890", "abcdefghijklmnopqrstuvwxyz_-ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890")]
        [TestCase("$_StartingWithUnderscore", "_StartingWithUnderscore")]
        //TODO: Are ${x-} and ${y_} valid variables (i.e. ending in - or _ )
        public void ParseReferenceWithVariable(string input, string variableName)
        {
            var reference = Parse(input, x => x.reference(), LexerInitialState);
            Assert.That(reference, Is.Not.Null);
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var variable = reference?.referenceBody()?.variable();
            Assert.That(variable, Is.Not.Null);

            Assert.That(variable.IDENTIFIER().GetText(), Is.EqualTo(variableName));
        }

        [TestCase("$foo.dog", "dog")]
        [TestCase("$!bar.cat", "cat")]
        [TestCase("${foo.fish}", "fish")]
        [TestCase("$!{bar.bear}", "bear")]
        public void ParseReferenceWithProperty(string input, string propertyName)
        {
            var reference = Parse(input, x => x.reference(), LexerInitialState);
            Assert.That(reference, Is.Not.Null);
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = reference.referenceBody()?.propertyInvocation().Single();
            Assert.That(property, Is.Not.Null);
            Assert.That(property.IDENTIFIER()?.GetText(), Is.EqualTo(propertyName));
        }


        [TestCase("$foo.dog()", "dog")]
        [TestCase("$!bar.cat()", "cat")]
        [TestCase("${foo.fish()}", "fish")]
        [TestCase("$!{bar.bear()}", "bear")]
        public void ParseReferenceWithMethod(string input, string methodName)
        {
            var reference = Parse(input, x => x.reference(), LexerInitialState);
            Assert.That(reference, Is.Not.Null);
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var method = reference.referenceBody()?.methodInvocation().Single();
            Assert.That(method, Is.Not.Null);
            Assert.That(method.IDENTIFIER()?.GetText(), Is.EqualTo(methodName));
            Assert.That(method.argument_list(), Is.Not.Null);
        }

        [TestCase("$!alpha.bravo().charlie()")]
        [TestCase("$!variable.Undefined.AnotherUndefined()")]
        public void ParsesReferenceWithMultipleParts(string input)
        {
            var reference = Parse(input, x => x.reference(), LexerInitialState);

            Assert.That(reference, Is.Not.Null);
            Assert.That(reference.GetText(), Is.EqualTo(input));
        }

    }
}
