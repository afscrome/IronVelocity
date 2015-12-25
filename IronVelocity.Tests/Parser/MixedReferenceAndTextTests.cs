using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Parser
{
    public class MixedReferenceAndTextTests : ParserTestBase
    {
        [TestCase("$test.stuff()($test", "$test.stuff()", "(", "$test")]
        [TestCaseSource("TwoReferencesWithPotentiallyAmbigiousCharcterInBetween")]
        public void ParseTwoReferenceswithTextInBetween(string input, string reference1, string text, string reference2)
        {
            var result = Parse(input, x => x.block());
            var textNode = result.rawText().Single();
            Assert.That(textNode.GetText(), Is.EqualTo(text));

            var referenceTexts = result.reference().Select(x => x.GetText());

            Assert.That(referenceTexts, Contains.Item(reference1));
            Assert.That(referenceTexts, Contains.Item(reference2));
        }

        public IEnumerable<TestCaseData> TwoReferencesWithPotentiallyAmbigiousCharcterInBetween()
        {
            string input;
            foreach (var text in Samples.PotentiallyAmbigiousReferenceDelimiters)
            {
                foreach (var reference1 in Samples.References)
                {
                    foreach (var reference2 in Samples.References)
                    {
                        input = reference1 + text + reference2;
                        yield return new TestCaseData(input, reference1, text, reference2)
                            .SetName("ParseTwoReferencesWithTextInBetween - " + input);
                    }
                }
            }
        }
    }
}
