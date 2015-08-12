using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    class ReferenceTests : ParserTestBase
    {
        [TestCase("$foo")]
        [TestCase("$!bar")]
        [TestCase("${foo}")]
        [TestCase("$!{bar}")]
        [TestCase("$CAPITAL")]
        [TestCase("$MiXeDCaSe")]
        [TestCase("$WithNumbers")]
        public void VariableReferences(string input)
        {
            var result = ParseEnsuringNoErrors(input);
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            Assert.That(flattened, Has.Exactly(1).InstanceOf<VelocityParser.ReferenceContext>());
        }

        [TestCase("$foo.dog", "dog")]
        [TestCase("$!bar.cat", "cat")]
        [TestCase("${foo.fish}", "fish")]
        [TestCase("$!{bar.bear}", "bear")]
        public void Property(string input, string propertyName)
        {
            PrintTokens(input);
            var result = ParseEnsuringNoErrors(input);
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            var reference = flattened.OfType<VelocityParser.ReferenceContext>().Single();
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = flattened.OfType<VelocityParser.Property_invocationContext>().Single();
            Assert.That(property.GetText(), Is.EqualTo(propertyName));
        }


        [TestCase("$foo.dog()", "dog")]
        [TestCase("$!bar.cat()", "cat")]
        [TestCase("${foo.fish()}", "fish")]
        [TestCase("$!{bar.bear()}", "bear")]
        public void ZeroArgumentMethod(string input, string methodName)
        {
            PrintTokens(input);
            var result = ParseEnsuringNoErrors(input);
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            var reference = flattened.OfType<VelocityParser.ReferenceContext>().Single();
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = flattened.OfType<VelocityParser.Method_invocationContext>().Single();
            Assert.That(property.GetText(), Is.EqualTo(methodName + "()"));
        }

        [TestCaseSource("TwoReferenceTestCaseData")]
        public void TwoReferences(string reference1, string reference2)
        {
            var input = reference1 + reference2;
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);
            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());

            var references = flattened.OfType<VelocityParser.ReferenceContext>()
                .Select(x => x.GetText())
                .ToList();

            Assert.That(references, Contains.Item(reference1));
            Assert.That(references, Contains.Item(reference2));
        }

        public void TwoReferenceswithTextInBetween(string reference1, string text, string reference2)
        {
            var input = reference1 + text + reference2;
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

        public IEnumerable<TestCaseData> TwoReferenceTestCaseData()
        {
            foreach (var left in Samples.References)
            {
                foreach (var right in Samples.References)
                {
                    yield return new TestCaseData(left, right)
                        .SetName($"Two References - '{left}', '{right}'");
                }
            }
        }
    }
}
