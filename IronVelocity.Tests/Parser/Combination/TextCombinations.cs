using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace IronVelocity.Tests.Parser.Combination
{
    public class TextCombinationTests : ParserTestBase
    {
        [TestCaseSource(nameof(TextCombinationTestCases))]
        public void TextCombinationTest(string input, Type expectedNodeType, string expectedText)
        {
            var result = Parse(input, x => x.block());

            Assert.That(result.ChildCount, Is.EqualTo(1));

            var node = result.GetChild(0);

            Assert.That(node, Is.TypeOf(expectedNodeType));
            Assert.That(node.GetText(), Is.EqualTo(expectedText));
        }

        public IEnumerable<TestCaseData> TextCombinationTestCases()
        {
            var textTypes = new[] { SampleType.ReferenceLikeText, SampleType.AmbigiousDelimiters };

            foreach (var leftType in textTypes)
            {
                foreach (var rightType in textTypes)
                {
                    foreach (var left in Samples.GetSamples(leftType))
                    {
                        foreach (var right in Samples.GetSamples(rightType))
                        {
                            if (Samples.MayBeProblematicToCombine(left, right))
                                continue;

                            var input = left + right;
                            yield return new TestCaseData(input, typeof(VelocityParser.TextContext), input)
                                .SetName($"zCombination Test - {leftType}, {rightType} - {input}");
                        }
                    }
                }
            }
        }
    }
}
