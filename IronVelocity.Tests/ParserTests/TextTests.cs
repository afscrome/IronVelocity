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

        [TestCase("$")]
        [TestCase("$$")]
        [TestCase("$$$")]
        [TestCase("$1.2")]
        [TestCase("$()")]
        [TestCase("$!")]
        [TestCase("$!{")]
        public void TextWhichLooksLikeReference(string input)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Text);
            var result = parser.Parse();

            Assert.That(parser.HasReachedEndOfFile);
            Assert.That(result.Children, Has.Length.EqualTo(1));
            Assert.That(result.Children, Has.All.InstanceOf<TextNode>());

            var textNode = (TextNode)result.Children[0];
            Assert.That(textNode.Content, Is.EqualTo(input));
        }

        [TestCase("$$x")]
        [TestCase("${$x")]
        [TestCase("$!$x")]
        [TestCase("$!{$x")]
        public void ReferenceLikeTextPreceedingAReference(string input)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Text);
            var result = parser.Parse();

            Assert.That(parser.HasReachedEndOfFile);
            Assert.That(result.Children, Has.Length.EqualTo(2));
            Assert.That(result.Children[0], Is.InstanceOf<TextNode>());
            Assert.That(result.Children[1], Is.InstanceOf<ReferenceNode>());
        }


        [TestCase("$test.", ".")]
        [TestCase("$test..", "..")]
        [TestCase("$test.stuff(", "(", Ignore = true, IgnoreReason = "Not yet implemented")] // '$test.stuff(EOF' is treated as text.  If there's  
        public void ReferenceLikeTextAfterAReference(string input, string textSuffix)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Text);
            var result = parser.Parse();

            Assert.That(parser.HasReachedEndOfFile);
            Assert.That(result.Children, Has.Length.EqualTo(2));
            Assert.That(result.Children[0], Is.InstanceOf<ReferenceNode>());
            Assert.That(result.Children[1], Is.InstanceOf<TextNode>());

            var textNode = (TextNode)result.Children[1];
            Assert.That(textNode.Content, Is.EqualTo(textSuffix));
        }
    }
}
