using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using IronVelocity.Parser;

namespace IronVelocity.Tests.Parser
{
    public class TextTests : ParserTestBase
    {
        [TestCase("Hello World")]
        [TestCaseSource("TextThatLooksLikeReferenceData")]
        public void ParseText(string input)
        {
            var text = CreateParser(input).text();
            Assert.That(text.GetText(), Is.EqualTo(input));
        }


        public IEnumerable<TestCaseData> TextThatLooksLikeReferenceData()
        {
            return Samples.ReferenceLikeText.Select(x => new TestCaseData(x).SetName("ParseTextThatLooksLikeReference - " + x));
        }
    }
}
