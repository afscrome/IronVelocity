using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.ParserTests
{
    public class TextTests
    {
        [Test]
        public void Text()
        {
            var parser = new VelocityParserWithStatistics("Hello World", LexerState.Text);
            var result = parser.Parse();

            Assert.That(parser.HasReachedEndOfFile);
            Assert.That(result.Children, Has.Length.EqualTo(1));
            Assert.That(result.Children[0], Is.TypeOf<TextNode>());
        }

        [Test]
        public void TextWithReference()
        {
            var parser = new VelocityParserWithStatistics("Hello $World", LexerState.Text);
            var result = parser.Parse();

            Assert.That(parser.HasReachedEndOfFile);
            Assert.That(result.Children, Has.Length.EqualTo(2));
            Assert.That(result.Children[0], Is.TypeOf<TextNode>());
            Assert.That(result.Children[1], Is.TypeOf<ReferenceNode>());
        }

        [TestCase("Hello${adverb}World")]
        [TestCase("Hello ${adverb} World")]
        [TestCase("Hello $adverb World")]
        public void TextWithReferenceThenTextAgain(string input)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Text);
            var result = parser.Parse();

            Assert.That(parser.HasReachedEndOfFile);
            Assert.That(result.Children, Has.Length.EqualTo(3));
            Assert.That(result.Children[0], Is.TypeOf<TextNode>());
            Assert.That(result.Children[1], Is.TypeOf<ReferenceNode>());
            Assert.That(result.Children[2], Is.TypeOf<TextNode>());
        }
    }
}
