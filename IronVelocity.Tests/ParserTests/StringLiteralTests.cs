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
    public class StringLiteralTests
    {
        [TestCase("'HelloWorld'", "HelloWorld", false)]
        [TestCase("\"Foo Bar\"", "Foo Bar", true)]
        [TestCase("\"Hello $baz\"", "Hello $baz", true)]
        [TestCase("\"Mark's pen\"", "Mark's pen", true)]
        [TestCase("'Joe said \"Hello\"'", "Joe said \"Hello\"", false)]
        public void StringLiteral(string input, string expectedValue, bool isInterpolated)
        {
            var parser = new VelocityParserWithStatistics(input);
            var result = parser.Expression();

            if (isInterpolated)
                Assert.That(parser.InterpolatedStringCallCount, Is.EqualTo(1));
            else
                Assert.That(parser.StringLiteralCallCount, Is.EqualTo(1));

            Assert.That(parser.ExpressionCallCount, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

            Assert.That(result, Is.TypeOf<StringNode>());
            var stringNode = (StringNode)result;
            Assert.That(stringNode.Value, Is.EqualTo(expectedValue));
            Assert.That(stringNode.IsInterpolated, Is.EqualTo(isInterpolated));
        }

    }
}
