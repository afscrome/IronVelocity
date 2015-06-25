using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using NUnit.Framework;
using System.Linq;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class ArgumentListTests
    {
        [TestCase("$door")]
        [TestCase(" $door")]
        [TestCase("$door ")]
        [TestCase(" $door ")]
        [TestCase("	$door")]
        [TestCase("\t$door ")]
        [TestCase("$door\t")]
        [TestCase("\t$door")]
        [TestCase("\t$door\t")]
        [TestCase(" \t \t $door \t \t")]
        public void ParseArgumentsWithWhitspace(string input)
        {
            var parser = new VelocityParser(input);

            var result = parser.ArgumentList(TokenKind.EndOfFile);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Arguments, Is.Not.Null);
            Assert.That(result.Arguments.Count == 1);

            var arg = result.Arguments.Single();
            Assert.That(arg, Is.TypeOf<ReferenceNode>());
            var reference = (ReferenceNode)arg;
            Assert.That(reference.Value, Is.TypeOf<Variable>());

            var variable = reference.Value as Variable;
            Assert.That(variable.Name, Is.EqualTo("door"));
            Assert.That(parser.HasReachedEndOfFile);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\t \t \t")]
        public void ParseEmptyArgumentList(string input)
        {
            var parser = new VelocityParser(input);
            var result = parser.ArgumentList(TokenKind.EndOfFile);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Arguments, Is.Not.Null);
            Assert.That(result.Arguments.Count, Is.EqualTo(0));
            Assert.That(parser.HasReachedEndOfFile);
        }

        [Test]
        public void ParseMultipleArguments()
        {
            var input = "$cat, $mat";
            var parser = new VelocityParser(input);

            var result = parser.ArgumentList(TokenKind.EndOfFile);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Arguments, Is.Not.Null);
            Assert.That(result.Arguments.Count, Is.EqualTo(2));

            var firstArg = result.Arguments[0];
            Assert.That(firstArg, Is.TypeOf<ReferenceNode>());
            var reference = (ReferenceNode)firstArg;
            Assert.That(reference.Value, Is.TypeOf<Variable>());

            var variable = reference.Value as Variable;
            Assert.That(variable.Name, Is.EqualTo("cat"));

            var secondArg = result.Arguments[1];
            Assert.That(firstArg, Is.TypeOf<ReferenceNode>());
            var secondReference = (ReferenceNode)secondArg;
            Assert.That(secondReference.Value, Is.TypeOf<Variable>());

            var secondVariable = secondReference.Value as Variable;
            Assert.That(secondVariable.Name, Is.EqualTo("mat"));
            Assert.That(parser.HasReachedEndOfFile);
        }
    }
}
