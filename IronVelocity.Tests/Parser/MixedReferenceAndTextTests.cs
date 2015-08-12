using IronVelocity.Parser;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Parser
{
    public class MixedReferenceAndTextTests : ParserTestBase
    {
        [TestCaseSource("ReferenceMixedWithReferenceLikeText")]
        [TestCaseSource("ReferenceFollowedByPotentiallyAmbigiousCharcter")]
        public void TextAndReference(string input, string expectedText, string expectedReference)
        {
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            var referenceNode = flattened.OfType<VelocityParser.ReferenceContext>().Single();

            Assert.That(textNode.GetText(), Is.EqualTo(expectedText));
            Assert.That(referenceNode.GetText(), Is.EqualTo(expectedReference));
        }

        [TestCaseSource("TwoReferencesWithPotentiallyAmbigiousCharcterInBetween")]
        public void TwoReferenceswithTextInBetween(string input, string reference1, string text, string reference2)
        {
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            Assert.That(textNode.GetText(), Is.EqualTo(text));

            var references = flattened.OfType<VelocityParser.ReferenceContext>()
                .Select(x => x.GetText())
                .ToList();

            Assert.That(references, Contains.Item(reference1));
            Assert.That(references, Contains.Item(reference2));
        }

        public IEnumerable<TestCaseData> ReferenceMixedWithReferenceLikeText()
        {
            string input;
            foreach (var text in Samples.ReferenceLikeText)
            {
                foreach (var reference in Samples.References)
                {
                    input = text + reference;
                    yield return new TestCaseData(input, text, reference)
                        .SetName("TextFollowedByReference - " + input);

                    input = reference + text;
                    yield return new TestCaseData(input, text, reference)
                        .SetName("ReferenceFollowedByText - " + input);

                }
            }
        }



        public IEnumerable<TestCaseData> ReferenceFollowedByPotentiallyAmbigiousCharcter()
        {
            string input;
            foreach (var text in Samples.PotentiallyAmbigiousReferenceDelimiters)
            {
                foreach (var reference in Samples.References)
                {
                    input = reference + text;
                    yield return new TestCaseData(input, text, reference)
                        .SetName("ReferenceFollowedByPotentiallyAmbigiousCharcter - " + input);
                }
            }
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
