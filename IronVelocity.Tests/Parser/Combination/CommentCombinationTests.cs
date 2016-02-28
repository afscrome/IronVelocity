using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Parser.Combination
{
    [TestFixture]
    public class CommentCombinationTests : ParserTestBase
    {
        [TestCaseSource(nameof(CommentCombinationTestCases))]
        public void CommentCombination(string input, string leftText, string rightText, Type leftNodeType, Type rightNodeType)
        {
            var result = Parse(input, x => x.block());

            Assert.That(result.ChildCount, Is.EqualTo(2));

            var left = result.GetChild(0);
            var right = result.GetChild(1);

            Assert.That(left, Is.TypeOf(leftNodeType));
            Assert.That(left.GetText(), Is.EqualTo(leftText));

            Assert.That(right, Is.TypeOf(rightNodeType));
            Assert.That(right.GetText(), Is.EqualTo(rightText));
        }

        public IEnumerable<TestCaseData> CommentCombinationTestCases()
        {
            var results = Enumerable.Empty<TestCaseData>();
            foreach (var sampleType in new[] { SampleType.AmbigiousDelimiters, SampleType.ReferenceLikeText, SampleType.Reference })
            {
                results = results.Concat(EnumerateSampleCombinations(sampleType, SampleType.BlockComment))
                                 .Concat(EnumerateSampleCombinations(SampleType.BlockComment, sampleType));
            }
            return results;
        }

        private IEnumerable<TestCaseData> EnumerateSampleCombinations(SampleType leftType, SampleType rightType)
        {
            foreach (var left in Samples.GetSamples(leftType))
            {
                var leftNodeType = Samples.GetSampleParserNodeType(leftType);
                foreach (var right in Samples.GetSamples(rightType))
                {
                    var rightNodeType = Samples.GetSampleParserNodeType(rightType);
                    if (Samples.MayBeProblematicToCombine(left, right))
                        continue;

                    var input = left + right;
                    yield return new TestCaseData(input, left, right, leftNodeType, rightNodeType )
                            .SetName($"zCombination Test - {leftType}, {rightType} - {input}");
                }
            }
        }
    }
}
