using IronVelocity.CodeAnalysis.Binding;
using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.CodeAnalysis.Tests
{
    public class EvaluatorTests
    {
        [TestCase("0", 0)]
        [TestCase("+3", 3)]
        [TestCase("-97", -97)]
        [TestCase("2147483647", int.MaxValue)]
        [TestCase("-2147483647", int.MinValue + 1)]
        [TestCase("--7", 7)]
        [TestCase("++3", 3)]
        [TestCase("-+6", -6)]
        [TestCase("-+-+-+1", -1)]
        public void NumberTest(string input, int expectedValue)
        {
            Assert.That(EvaluateString(input), Is.EqualTo(expectedValue));
        }

        [TestCase("1+3", 4)]
        [TestCase("4-9", -5)]
        [TestCase("8*9", 72)]
        [TestCase("98/2", 49)]
        [TestCase("5/4", 1)]
        [TestCase("4 + 5", 9)]
        [TestCase("1+7-4", 4)]
        [TestCase("1-7+4", -2)]
        [TestCase("8*4/2", 16)]
        [TestCase("8/4*2", 4)]
        [TestCase("3+7*1", 10)]
        [TestCase("3*7+1", 22)]
        [TestCase("(0)", 0)]
        [TestCase("3*(4+5)", 27)]
        public void NumericExpressionTests(string input, int expectedValue)
        {
            Assert.That(EvaluateString(input), Is.EqualTo(expectedValue));
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void BooleanTest(string input, bool expectedValue)
        {
            Assert.That(EvaluateString(input), Is.EqualTo(expectedValue));
        }

        private object? EvaluateString(string input)
        {
            var lexer = new Lexer(input);

            var tokens = lexer.ReadAllTokens();
            Warn.If(lexer.Diagnostics, Is.Not.Empty);

            var parser = new Parser(tokens);
            var syntaxTree = parser.Parse();

            Warn.If(syntaxTree.Diagnostics, Is.Not.Empty);

            var binder = new Binder();
            var boundExpression = binder.BindExpression(syntaxTree.Root);
            Warn.If(binder.Diagnostics, Is.Not.Empty);

            var evaluator = new Evaluator(boundExpression);
            return evaluator.Evaluate();
        }
    }
}