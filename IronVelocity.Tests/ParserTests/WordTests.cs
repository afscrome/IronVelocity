using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Parser.AST;
using IronVelocity.Parser;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class WordTests
    {
        [TestCase("in")]
        [TestCase("TRUE")]
        [TestCase("FALSE")]
        public void Word(string name)
        {
            var parser = new VelocityParser(name);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<WordNode>());
            var node = (WordNode)result;

            Assert.That(node.Name, Is.EqualTo(name));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

    }
}
