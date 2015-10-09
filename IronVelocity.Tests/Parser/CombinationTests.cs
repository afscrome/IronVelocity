using Antlr4.Runtime.Tree;
using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Parser
{
    [TestFixture]
    public class CombinationTests : ParserTestBase
    {
        private readonly Dictionary<string, IEnumerable<string>> _contentTypeSamples = new Dictionary<string, IEnumerable<string>>
        {
            ["ReferenceLikeText"] = Samples.ReferenceLikeText,
            ["ReferenceDelimiters"] = Samples.PotentiallyAmbigiousReferenceDelimiters,
            ["Reference"] = Samples.References,
            ["Comment"] = Samples.Comments,
        };

        private readonly Dictionary<string, Type> _contentTypeNodeTypes = new Dictionary<string, Type>
        {
            ["ReferenceLikeText"] = typeof(VelocityParser.TextContext),
            ["ReferenceDelimiters"] = typeof(VelocityParser.TextContext),
            ["Reference"] = typeof(VelocityParser.ReferenceContext),
            ["Comment"] = typeof(VelocityParser.CommentContext)
        };

        [TestCaseSource("CombinationTestSource")]
        public void CombinationTest(string input, Type expectedNodeType1, string expectedText1, Type expectedNodeType2, string expectedText2)
        {
            var result = Parse(input, x => x.template());

            var flattened = FlattenParseTree(result);

            if (expectedNodeType1 != expectedNodeType2)
                TwoDifferentNodes(flattened, expectedNodeType1, expectedText1, expectedNodeType2, expectedText2);
            else if (expectedNodeType1 == typeof(VelocityParser.TextContext))
                TwoTextNodes(flattened, input);
            else
                SameTwoNonTextNodes(flattened, expectedNodeType1, expectedText1, expectedText2);
        }

        private void TwoDifferentNodes(IEnumerable<IParseTree> flattened, Type expectedNodeType1, string expectedText1, Type expectedNodeType2, string expectedText2)
        {
            var node1 = flattened.Single(x => x.GetType() == expectedNodeType1);
            var node2 = flattened.Single(x => x.GetType() == expectedNodeType2);

            Assert.That(node1.GetText(), Is.EqualTo(expectedText1));
            Assert.That(node2.GetText(), Is.EqualTo(expectedText2));
        }

        private void TwoTextNodes(IEnumerable<IParseTree> flattened, string input)
        {
            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();

            Assert.That(textNode.GetText(), Is.EqualTo(input));
        }

        public void SameTwoNonTextNodes(IEnumerable<IParseTree> flattened, Type nodetype, string expectedText1, string expectedText2)
        {
            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());

            var references = flattened
                .Where(x => x.GetType() == nodetype)
                .Select(x => x.GetText());

            Assert.That(references, Is.EquivalentTo(new[] { expectedText1, expectedText2 }));
        }

        public IEnumerable<TestCaseData> CombinationTestSource()
        {
            foreach (var leftContentType in _contentTypeSamples)
            {
                foreach (var rightContentType in _contentTypeSamples)
                {
                    var leftNodeType = _contentTypeNodeTypes[leftContentType.Key];
                    var rightNodeType = _contentTypeNodeTypes[rightContentType.Key];

                    foreach (var leftValue in leftContentType.Value)
                    {
                        foreach (var rightValue in rightContentType.Value)
                        {
                            // If the left is a single hash, and the right starts with a hash
                            // Then the two will combine to form a single comment, so we ignore those
                            // scenarios
                            if (leftValue == "#" && rightValue.StartsWith("#"))
                                continue;

                            var input = leftValue + rightValue;
                            yield return new TestCaseData(input, leftNodeType, leftValue, rightNodeType, rightValue)
                                .SetName($"zCombination Test - {leftContentType.Key}, {rightContentType.Key} - {input}");
                        }
                    }
                }
            }
        }


        private IEnumerable<IParseTree> FlattenParseTree(IParseTree parseTree)
        {
            Stack<IParseTree> nodes = new Stack<IParseTree>();
            nodes.Push(parseTree);

            do
            {
                var node = nodes.Pop();
                yield return node;
                for (int i = 0; i < node.ChildCount; i++)
                {
                    nodes.Push(node.GetChild(i));
                }
            }
            while (nodes.Any());
        }
    }
}
