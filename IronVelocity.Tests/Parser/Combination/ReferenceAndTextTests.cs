using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Parser.Combination
{
    public class ReferenceAndTextTests : ParserTestBase
    {
        [TestCase("$test.stuff()($test", "$test.stuff()", "(", "$test")]
        [TestCaseSource(nameof(TwoReferencesWithPotentiallyAmbigiousCharcterInBetween))]
        public void ParseTwoReferenceswithTextInBetween(string input, string reference1, string text, string reference2)
        {
            var result = Parse(input, x => x.block());
            var textNode = result.text().Single();
            Assert.That(textNode.GetText(), Is.EqualTo(text));

            var referenceTexts = result.reference().Select(x => x.GetText());

            Assert.That(referenceTexts, Contains.Item(reference1));
            Assert.That(referenceTexts, Contains.Item(reference2));
        }

        [TestCaseSource(nameof(CombinationTestSource))]
        public void CombinationTest(string input, Type expectedLeftNodeType, Type expectedRightNodeType, string leftExpectedText, string rightExpectedText)
        {
            var result = Parse(input, x => x.block());
            Assert.That(result.ChildCount, Is.EqualTo(2));

            var left = result.GetChild(0);
            var right = result.GetChild(1);

            Assert.That(left, Is.TypeOf(expectedLeftNodeType));
            Assert.That(right, Is.TypeOf(expectedRightNodeType));

            Assert.That(left.GetText(), Is.EqualTo(leftExpectedText));
            Assert.That(right.GetText(), Is.EqualTo(rightExpectedText));
        }


        public IEnumerable<TestCaseData> TwoReferencesWithPotentiallyAmbigiousCharcterInBetween()
        {
            foreach (var text in Samples.GetSamples(SampleType.AmbigiousDelimiters))
            {
                foreach (var left in Samples.GetSamples(SampleType.Reference))
                {
                    foreach (var right in Samples.GetSamples(SampleType.Reference))
                    {
                        var input = left + text + right;
                        yield return new TestCaseData(input, left, text, right)
                            .SetName("ParseTwoReferencesWithTextInBetween - " + input);
                    }
                }
            }
        }

        public IEnumerable<TestCaseData> CombinationTestSource()
        {
            return CreateCombinationTestsWithTwoNodeResult(SampleType.Reference, SampleType.AmbigiousDelimiters)
                .Concat(CreateCombinationTestsWithTwoNodeResult(SampleType.Reference, SampleType.ReferenceLikeText))
                .Concat(CreateCombinationTestsWithTwoNodeResult(SampleType.ReferenceLikeText, SampleType.Reference))
                .Concat(CreateCombinationTestsWithTwoNodeResult(SampleType.AmbigiousDelimiters, SampleType.Reference))
                .Concat(CreateCombinationTestsWithTwoNodeResult(SampleType.Reference, SampleType.Reference));
        }

        public IEnumerable<TestCaseData> CreateCombinationTestsWithTwoNodeResult(SampleType leftType, SampleType rightType)
        {
            var leftNodeType = Samples.GetSampleParserNodeType(leftType);
            var rightNodeType = Samples.GetSampleParserNodeType(rightType);

            foreach (var left in Samples.GetSamples(leftType))
            {
                foreach (var right in Samples.GetSamples(rightType))
                {
                    if (left.EndsWith("#") && (right.StartsWith("#") || right.StartsWith("{")))
                        continue;

                    var input = left + right;
                    yield return new TestCaseData(input, leftNodeType, rightNodeType, left, right)
                        .SetName($"zCombination Test - {leftType}, {rightType} - {input}");
                }
            }
        }
    }
}
