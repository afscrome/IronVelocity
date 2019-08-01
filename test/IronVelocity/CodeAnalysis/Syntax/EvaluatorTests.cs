using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class EvaluatorTests
    {
        [TestCase("0", 0)]
        [TestCase("2147483647", 2147483647)]
        [TestCase("1+3", 4)]
        [TestCase("4-9", -5)]
        [TestCase("4 + 5", 9)]
        [TestCase("1+7-4", 4)]
        [TestCase("1-7+4", -2)]
        public void BasicTest(string input, int expectedValue)
        {
            var lexer = new Lexer(input);

            var tokens = lexer.ReadAllTokens();
            Warn.If(lexer.Diagnostics, Is.Not.Empty);

            var parser = new IronVelocity.CodeAnalysis.Syntax.Parser(tokens);
            var syntaxTree = parser.Parse();

            Warn.If(syntaxTree.Diagnostics, Is.Not.Empty);

            var evaluator = new Evaluator(syntaxTree);

            Assert.That(evaluator.Evaluate(), Is.EqualTo(expectedValue));
        }
    }
}