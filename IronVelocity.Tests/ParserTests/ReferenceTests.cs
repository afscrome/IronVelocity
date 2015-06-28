using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using NUnit.Framework;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class ReferenceTests
    {
        [TestCase("$foo", false, false, "foo")]
        [TestCase("$!bar", true, false, "bar")]
        [TestCase("${baz}", false, true, "baz")]
        [TestCase("$!{foobar}", true, true, "foobar")]
        public void ParseVariable(string input, bool isSilent, bool isFormal, string variableName)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Vtl);
            var result = parser.Expression();

            Assert.That(parser.ReferenceCallCount, Is.EqualTo(1));
            Assert.That(parser.VariableCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

            Assert.That(result, Is.TypeOf<ReferenceNode>());
            var reference = (ReferenceNode)result;
            Assert.That(reference.IsSilent, Is.EqualTo(isSilent));
            Assert.That(reference.IsFormal, Is.EqualTo(isFormal));

            Assert.That(reference.Value, Is.TypeOf<Variable>());
            var variable = (Variable)reference.Value;
            Assert.That(variable.Name, Is.EqualTo(variableName));
        }

        [TestCase("$foo.boo", false, false, "boo")]
        [TestCase("$!bar.Koopa", true, false, "Koopa")]
        [TestCase("${baz.BOB-BOMB}", false, true, "BOB-BOMB")]
        [TestCase("$!{foobar.BlOoPeR}", true, true, "BlOoPeR")]
        public void ParseProperty(string input, bool isSilent, bool isFormal, string propertyName)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Vtl);
            var result = parser.Expression();

            Assert.That(parser.ReferenceCallCount, Is.EqualTo(1));
            Assert.That(parser.VariableCallCount, Is.EqualTo(1));
            Assert.That(parser.PropertyCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

            Assert.That(result, Is.TypeOf<ReferenceNode>());
            var reference = (ReferenceNode)result;

            Assert.That(reference.IsSilent, Is.EqualTo(isSilent));
            Assert.That(reference.IsFormal, Is.EqualTo(isFormal));

            Assert.That(reference.Value, Is.TypeOf<Property>());
            var property = (Property)reference.Value;
            Assert.That(property.Name, Is.EqualTo(propertyName));
        }

        [TestCase("$foo.red()", false, false, "red")]
        [TestCase("$!bar.Yellow()", true, false, "Yellow")]
        [TestCase("${baz.PINKY_BROWN()}", false, true, "PINKY_BROWN")]
        [TestCase("$!{foobar.ScArLEt()}", true, true, "ScArLEt")]
        public void ParseMethodWithNoArguments(string input, bool isSilent, bool isFormal, string methodName)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Vtl);
            var result = parser.Expression();

            Assert.That(parser.ReferenceCallCount, Is.EqualTo(1));
            Assert.That(parser.VariableCallCount, Is.EqualTo(1));
            Assert.That(parser.MethodCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

            Assert.That(result, Is.TypeOf<ReferenceNode>());
            var reference = (ReferenceNode)result;

            Assert.That(reference.IsSilent, Is.EqualTo(isSilent));
            Assert.That(reference.IsFormal, Is.EqualTo(isFormal));

            Assert.That(reference.Value, Is.TypeOf<Method>());
            var method = (Method)reference.Value;
            Assert.That(method.Name, Is.EqualTo(methodName));
        }

        /* TODO: handle invalid references - Exception? Treat as Text?
         * $
         * $$
         * ${
         * ${}
         * ${stuff
         * $!{
        */
    }
}
