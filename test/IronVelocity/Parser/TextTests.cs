using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class TextTests : ParserTestBase
    {
        [TestCase("Hello World")]
        [TestCaseSource(nameof(TextTests.TextThatLooksLikeReferenceData))]
        public void ParseText(string input)
        {
            var text = Parse(input, x => x.text());
            Assert.That(text.GetText(), Is.EqualTo(input));
        }


        public static IEnumerable<TestCaseData> TextThatLooksLikeReferenceData()
            => Samples.GetSamples(SampleType.ReferenceLikeText)
                .Select(x => new TestCaseData(x).SetName("ParseTextThatLooksLikeReference - " + x));
    }
}
