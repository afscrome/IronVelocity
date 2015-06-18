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
        public void StringLiteral(string input, string expected, bool interpolated)
        {
            var parser = new VelocityParser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<StringNode>());
            var node = (StringNode)result;

            Assert.That(node.Value, Is.EqualTo(expected));
            Assert.That(node.IsInterpolated, Is.EqualTo(interpolated));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }
    }
}
