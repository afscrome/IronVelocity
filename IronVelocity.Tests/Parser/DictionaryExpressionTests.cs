using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    public class DictionaryExpressionTests : ParserTestBase
    {
        [TestCase("{}", 0)]
        [TestCase("{  }", 0)]
        [TestCase("{ key : $value }", 1)]
        [TestCase("{ key : $value, key2 : $value2 }", 2)]
        [TestCase("{ key : $value, key2 : $value2, foo : $bar, baz : $bizz}", 4)]
        public void ShouldParseDictionaryExpression(string input, int itemCount)
        {
            var result = (VelocityParser.DictionaryExpressionContext)Parse(input, x => x.expression(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.dictionaryEntry(), Has.Length.EqualTo(itemCount));
        }

        [TestCase("key:'value'", "key", "'value'")]
        [TestCase("AnotherKey : \"value\"", "AnotherKey", "\"value\"")]
        [TestCase("'string' : 123", "'string'", "123")]
        [TestCase("\"interpolatedString\" : true", "\"interpolatedString\"", "true")]
        public void ShouldParseDictionaryEntry(string input, string key, string value)
        {
            var result = Parse(input, x => x.dictionaryEntry(), VelocityLexer.ARGUMENTS);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Key.Text, Is.EqualTo(key));
            Assert.That(result.expression().GetText(), Is.EqualTo(value));
        }
    }
}
