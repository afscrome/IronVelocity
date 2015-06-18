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
        [TestCase(732)]
        [TestCase(-43)]
        public void IntegerLiteral(int input)
        {
            var parser = new VelocityParser(input.ToString());
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<IntegerNode>());

            var node = (IntegerNode)result;
            Assert.That(node.Value, Is.EqualTo(input));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

        [TestCase(83.23f)]
        [TestCase(-7.3f)]
        public void FloatingPointLiteral(float input)
        {
            var parser = new VelocityParser(input.ToString());
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<FloatingPointNode>());

            var node = (FloatingPointNode)result;
            Assert.That(node.Value, Is.EqualTo(input));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }
    }
}
