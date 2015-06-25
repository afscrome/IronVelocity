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
    public class ListExpressionTests
    {
        [TestCase("[]", 0)]
        [TestCase("[123]", 1)]
        [TestCase("['test',4.5,$variable,true]", 4)]
        public void ListHasExpectedNumberOfNestedExpressionCalls(string input, int elementCount)
        {
            var parser = new VelocityParserWithStatistics(input);
            var result = parser.Expression();

            Assert.That(parser.RangeOrListCallCount, Is.EqualTo(1));
            Assert.That(parser.ExpressionCallCount, Is.EqualTo(elementCount + 1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

            Assert.That(result, Is.TypeOf<ListExpressionNode>());
            var node = (ListExpressionNode)result;
            Assert.That(node.Values.Count, Is.EqualTo(elementCount));
        }

        [Test]
        public void NestedLists()
        {
            var input = "[[123]]";
            var parser = new VelocityParserWithStatistics(input);
            var result = parser.Expression();

            Assert.That(parser.RangeOrListCallCount, Is.EqualTo(2));
            Assert.That(parser.ExpressionCallCount, Is.EqualTo(3));
            Assert.That(parser.HasReachedEndOfFile, Is.True);


            Assert.That(result, Is.TypeOf<ListExpressionNode>());
            var outerList = (ListExpressionNode)result;

            Assert.That(outerList.Values.Count, Is.EqualTo(1));
            Assert.That(outerList.Values[0], Is.TypeOf<ListExpressionNode>());

            var innerList = (ListExpressionNode)outerList.Values[0];
            Assert.That(innerList.Values.Count, Is.EqualTo(1));

            Assert.That(innerList.Values[0], Is.TypeOf<IntegerLiteralNode>());
            var innerListValue = (IntegerLiteralNode)innerList.Values[0];

            Assert.That(innerListValue.Value, Is.EqualTo(123));
        }

    }
}
