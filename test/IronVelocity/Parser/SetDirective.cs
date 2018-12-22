using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class SetDirective : ParserTestBase
    {
        [TestCase("#set($x = 123)")]
        [TestCase("#set   ($x = 123)")]
        [TestCase("#set($output = $input.Property)")]
        [TestCase("#set($output = $input.ToString())")]
        [TestCase("#set($output = $input.ToString( ))")]
        [TestCase("#set($output = $input.ToString(123))")]
        [TestCase("#set($output = ($input.ToString(123, 'hello')))")]
        [TestCase("#{set}($foo = 'bar')")]
        public void ParseSetDirective(string input)
        {
            var result = Parse(input, x => x.setDirective());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetFullText(), Is.EqualTo(input));

            Assert.That(result.assignment(), Is.Not.Null);
        }
    }
}
