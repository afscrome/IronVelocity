using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser 
{
    public class SetDirective : ParserTestBase
    {
        [TestCase("#set($x = 123)")]
        [TestCase("#set   ($x = 123)")]
        public void ParseSetDirective(string input)
        {
            var result = Parse(input, x => x.set_directive());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.assignment(), Is.Not.Null);
        }
    }
}
