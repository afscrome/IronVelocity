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
        public void NumericExpression(string input)
        {
            var parser = new Parser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<NumericNode>());

            var numeric = (NumericNode)result;
            Assert.That(numeric.Value, Is.EqualTo(input));
            Assert.That(parser.HasReachedEndOfFile);
        }


    }
}
