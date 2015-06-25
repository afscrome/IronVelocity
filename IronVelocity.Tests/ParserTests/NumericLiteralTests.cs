using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class NumericLiteralTests
    {
        [TestCase("732", 732)]
        [TestCase("-43", -43)]
        public void IntegerLiteral(string input, int expectedValue)
        {
            var parser = new VelocityParserWithStatistics(input);
            var result = parser.Expression();

            Assert.That(parser.IntegerCallCount, Is.EqualTo(1));
            Assert.That(parser.ExpressionCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

            Assert.That(result, Is.TypeOf<IntegerLiteralNode>());
            var integerNode = (IntegerLiteralNode)result;
            Assert.That(integerNode.Value, Is.EqualTo(expectedValue));
        }

        [TestCase("83.23",83.23f)]
        [TestCase("-7.3", -7.3f)]
        [TestCase("42.0", 42f)]
        [TestCase("-98.0", -98f)]
        public void FloatingPointLiteral(string input, float expectedValue)
        {
            var parser = new VelocityParserWithStatistics(input);
            var result = parser.Expression();

            Assert.That(parser.FloatCallCount, Is.EqualTo(1));
            Assert.That(parser.ExpressionCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

            Assert.That(result, Is.TypeOf<FloatingPointLiteralNode>());
            var floatNode = (FloatingPointLiteralNode)result;
            Assert.That(floatNode.Value, Is.EqualTo(expectedValue));
        }

    }
}
