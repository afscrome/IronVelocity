using IronVelocity.Parser;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Parser
{
    public class MixedReferenceAndTextTests : ParserTestBase
    {

        [TestCaseSource("TwoReferencesWithPotentiallyAmbigiousCharcterInBetween")]
        public void TwoReferenceswithTextInBetween(string input, string reference1, string text, string reference2)
        {
            var result = CreateParser(input).template();
            var textNode = result.text().Single();
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
                            .SetName("TwoReferencesWithPotentiallyAmbigiousCharcterInBetween - " + input);
                    }
                }
            }
        }
    }
}
