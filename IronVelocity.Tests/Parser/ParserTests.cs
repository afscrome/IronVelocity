using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    using Parser = IronVelocity.Parser.Parser;
    [TestFixture]
    public class ParserTests
    {
        [TestCase("$foo", false, false, "foo")]
        [TestCase("$!bar", true, false, "bar")]
        [TestCase("${baz}", false, true, "baz")]
        [TestCase("$!{foobar}", true, true, "foobar")]
        public void ParseVariable(string input, bool isSilent, bool isFormal, string identifier)
        {
            var parser = new Parser(input);

            var result = parser.Reference();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSilent, Is.EqualTo(isSilent));
            Assert.That(result.IsFormal, Is.EqualTo(isFormal));
            Assert.That(result.Identifier, Is.EqualTo(identifier));
        }
        /* TODO: handle invalid references - Exception? Treat as Text?
         * $
         * $$
         * ${
         * ${}
         * ${stuff
         * $!{
        */
    }
}
