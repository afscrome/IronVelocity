using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    public class LiteralTests : ParserTestBase
    {
        [TestCase("#[[]]")]
        [TestCase("#[[Foo]]")]
        [TestCase("#[[$ref]]")]
        [TestCase("#[[#directive]]")]
        [TestCase("#[[${bad #{syntax]]")]
        [TestCase("#[[ Single Close ] Square ]]")]
        [TestCase("#[[ Double ] Close ] Square ]]")]
        public void ParseLiteral(string input)
        {
            var literal = Parse(input, x => x.literal());
            Assert.That(literal.GetText(), Is.EqualTo(input));
        }
    }
}
