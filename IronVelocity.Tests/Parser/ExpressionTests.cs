using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Parser.AST;

namespace IronVelocity.Tests.Parser
{
    using Parser = IronVelocity.Parser.Parser;
    [TestFixture]
    public class ExpressionTests
    {
        [TestCase("732")]
        [TestCase("83.23")]
        [TestCase("-43")]
        [TestCase("-7.3")]
        public void NumericExpression(string input)
        {
            var parser = new Parser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<NumericNode>());

            var numeric = (NumericNode)result;
            Assert.That(numeric.Value, Is.EqualTo(input));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

        [TestCase("'HelloWorld'", "HelloWorld", false)]
        [TestCase("\"Foo Bar\"", "Foo Bar", true)]
        [TestCase("\"Hello $baz\"", "Hello $baz", true)]
        public void StringExpression(string input, string expected, bool interpolated)
        {
            var parser = new Parser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<StringNode>());
            var stringNode = (StringNode)result;

            Assert.That(stringNode.Value, Is.EqualTo(expected));
            Assert.That(stringNode.IsInterpolated, Is.EqualTo(interpolated));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

    }
}
