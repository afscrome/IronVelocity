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
        public void NumericLiteral(string input)
        {
            var parser = new Parser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<NumericNode>());

            var node = (NumericNode)result;
            Assert.That(node.Value, Is.EqualTo(input));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

        [TestCase("'HelloWorld'", "HelloWorld", false)]
        [TestCase("\"Foo Bar\"", "Foo Bar", true)]
        [TestCase("\"Hello $baz\"", "Hello $baz", true)]
        public void StringLiteral(string input, string expected, bool interpolated)
        {
            var parser = new Parser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<StringNode>());
            var node = (StringNode)result;

            Assert.That(node.Value, Is.EqualTo(expected));
            Assert.That(node.IsInterpolated, Is.EqualTo(interpolated));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void BooleanLiteral(string input, bool expected)
        {
            var parser = new Parser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<BooleanNode>());
            var node = (BooleanNode)result;

            Assert.That(node.Value, Is.EqualTo(expected));
            Assert.That(parser.HasReachedEndOfFile, Is.True);

        }

        [TestCase("in")]
        [TestCase("TRUE")]
        [TestCase("FALSE")]
        public void WordLiteral(string name)
        {
            var parser = new Parser(name);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<WordNode>());
            var node = (WordNode)result;

            Assert.That(node.Name, Is.EqualTo(name));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }
    }
}
