using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Parser
{
    public class CommentTests : ParserTestBase
    {
        [TestCase("##Comment")]
        [TestCase("##$reference")]
        [TestCase("##set")]
        [TestCase("##Comment\r")]
        [TestCase("##Comment\n")]
        [TestCase("##Comment\r\n")]
        public void SimpleSingleLineComment(string input)
        {
            PrintTokens(input);

            var result = ParseEnsuringNoErrors(input);

            var comment = FlattenParseTree(result).OfType<VelocityParser.CommentContext>().Single();

            Assert.That(comment.GetText(), Is.EqualTo(input));
        }

        [Test]
        public void TextFollowedByComment()
        {
            string input = "hello ##world";
            var expectedText = "hello ";
            var expectedComment = "##world";

            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            var commentNode = flattened.OfType<VelocityParser.CommentContext>().Single();

            Assert.That(textNode.GetText(), Is.EqualTo(expectedText));
            Assert.That(commentNode.GetText(), Is.EqualTo(expectedComment));
        }

        [TestCase("##hello\r\nworld", "world", "##hello\r\n")]
        [TestCase("hello##world", "hello", "##world")]
        public void CommentAndText(string input, string expectedText, string expectedComment)
        {
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            var commentNode = flattened.OfType<VelocityParser.CommentContext>().Single();

            Assert.That(textNode.GetText(), Is.EqualTo(expectedText));
            Assert.That(commentNode.GetText(), Is.EqualTo(expectedComment));
        }

    }
}
