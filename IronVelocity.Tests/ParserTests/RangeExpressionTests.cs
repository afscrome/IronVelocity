
using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using NUnit.Framework;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class RangeExpressionTests
    {
        [Test]
        public void RangeExpressionParsesTwoNestedExpressions()
        {
            var input = "[$start..$end]";
            var parser = new VelocityParserWithStatistics(input, LexerState.Vtl);
            var result = parser.Expression();
            Assert.That(result, Is.TypeOf<BinaryExpressionNode>());

            Assert.That(parser.RangeOrListCallCount, Is.EqualTo(1));
            Assert.That(parser.ExpressionCallCount, Is.EqualTo(3));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

        [TestCase("[1..2]", 1, 2)]
        [TestCase("[-5..-9]", -5, -9)]
        [TestCase("[ 20 .. 7]", 20, 7)]
        [TestCase("[ -3 .. -9]", -3, -9)]
        public void RangeExpression(string input, int start, int end)
        {
            var parser = new VelocityParser(input, LexerState.Vtl);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<BinaryExpressionNode>());
            var node = (BinaryExpressionNode)result;

            Assert.That(node.Left, Is.TypeOf<IntegerLiteralNode>());
            Assert.That(node.Right, Is.TypeOf<IntegerLiteralNode>());

            var left = node.Left as IntegerLiteralNode;
            var right = node.Right as IntegerLiteralNode;

            Assert.That(left.Value , Is.EqualTo(start));
            Assert.That(right.Value, Is.EqualTo(end));

            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

    }
}
