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
        [TestCase("[1..2]", 1, 2)]
        [TestCase("[-5..-9]", 1, 2)]
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
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

    }
}
