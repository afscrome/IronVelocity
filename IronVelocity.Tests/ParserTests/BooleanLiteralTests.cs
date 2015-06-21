using IronVelocity.Parser.AST;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.ParserTests
{
    public class BooleanLiteralTests
    {
        [TestCase("true", true)]
        [TestCase("false", false)]
        public void BooleanLiteral(string input, bool expected)
        {
            var parser = new VelocityParserWithStatistics(input);
            var result = parser.Expression();

            Assert.That(parser.BooleanLiteralOrWordCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile);

            Assert.That(result, Is.TypeOf<BooleanNode>());
            var node = (BooleanNode)result;

            Assert.That(node.Value, Is.EqualTo(expected));
        }
    }
}
