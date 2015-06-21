using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using IronVelocity.Parser.AST;
using IronVelocity.Parser;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class RangeExpressionTests
    {
        [Test]
        public void RangeExpressionParsesTwoNestedExpressions()
        {
            var input = "[$start..$end]";
            var parser = new VelocityParserWithStatistics(input);
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
            var parser = new VelocityParser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<BinaryExpressionNode>());
            var node = (BinaryExpressionNode)result;

            Assert.That(node.Left, Is.TypeOf<IntegerNode>());
            Assert.That(node.Right, Is.TypeOf<IntegerNode>());

            var left = node.Left as IntegerNode;
            var right = node.Right as IntegerNode;

            Assert.That(left.Value , Is.EqualTo(start));
            Assert.That(right.Value, Is.EqualTo(end));

            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

    }
}
