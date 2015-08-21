﻿using IronVelocity.Parser;
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
        [TestCase("##")]
        [TestCase("##Comment")]
        [TestCase("##$reference")]
        [TestCase("##set")]
        [TestCase("##Comment\r")]
        [TestCase("##Comment\n")]
        [TestCase("##Comment\r\n")]
        public void SingleLineComment(string input) => SingleComment(input);

        [TestCase("#**#")]
        [TestCase("#*Multi Line*#")]
        [TestCase("#*Multi \r Line*#")]
        [TestCase("#*Multi \n Line*#")]
        [TestCase("#*Multi \r\n Line*#")]
        public void MultiLineComment(string input) => SingleComment(input);

        [TestCase("#**Formal*#")]
        [TestCase("#**Formal\r\nComment*#")]
        public void FormalComment(string input) => SingleComment(input);

        [TestCase("#*Outer #*Nested*# Outer*#")]
        [TestCase("#**Outer Formal #* Nested Informal *# Outer*#")]
        [TestCase("#*Outer Informal #** Nested Formal *# Outer*#")]
        [TestCase("#*#**#*#")]
        [TestCase("#*  #**#*#")]
        [TestCase("#*#**#   *#")]
        [TestCase("#* #*1*#  *#")]
        public void NestedComments(string input) => SingleComment(input);


        public void SingleComment(string input)
        {
            var result = CreateParser(input).template();

            var comment = FlattenParseTree(result).OfType<VelocityParser.CommentContext>().Single();

            Assert.That(comment.GetText(), Is.EqualTo(input));
        }

        [Test]
        public void TextFollowedByComment()
        {
            string input = "hello ##world";
            var expectedText = "hello ";
            var expectedComment = "##world";

            var result = CreateParser(input).template();

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
            var result = CreateParser(input).template();

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            var commentNode = flattened.OfType<VelocityParser.CommentContext>().Single();

            Assert.That(textNode.GetText(), Is.EqualTo(expectedText));
            Assert.That(commentNode.GetText(), Is.EqualTo(expectedComment));
        }

    }
}