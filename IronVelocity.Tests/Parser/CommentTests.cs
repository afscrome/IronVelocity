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
        [TestCase("##")]
        [TestCase("##Comment")]
        [TestCase("##$reference")]
        [TestCase("##set")]
        [TestCase("##Comment\r")]
        [TestCase("##Comment\n")]
        [TestCase("##Comment\r\n")]
        public void ParseSingleLineComment(string input) => SingleComment(input);

        [TestCase("#**#")]
        [TestCase("#*Multi Line*#")]
        [TestCase("#*Multi \r Line*#")]
        [TestCase("#*Multi \n Line*#")]
        [TestCase("#*Multi \r\n Line*#")]
        public void ParseMultiLineComment(string input) => SingleComment(input);

        [TestCase("#**Formal*#")]
        [TestCase("#**Formal\r\nComment*#")]
        public void ParseFormalComment(string input) => SingleComment(input);

        [TestCase("#*Outer #*Nested*# Outer*#")]
        [TestCase("#**Outer Formal #* Nested Informal *# Outer*#")]
        [TestCase("#*Outer Informal #** Nested Formal *# Outer*#")]
        [TestCase("#*#**#*#")]
        [TestCase("#*  #**#*#")]
        [TestCase("#*#**#   *#")]
        [TestCase("#* #*1*#  *#")]
        public void ParseNestedComments(string input) => SingleComment(input);


        public void SingleComment(string input)
        {
            var comment = CreateParser(input).comment();

            Assert.That(comment, Is.Not.Null);
            Assert.That(comment.GetText(), Is.EqualTo(input));
        }

        [TestCase("##hello\r\nworld", "world", "##hello\r\n")]
        [TestCase("hello##world", "hello", "##world")]
        public void ParseCommentAndText(string input, string expectedText, string expectedComment)
        {
            var result = CreateParser(input).template();

            var textNode = result.text().Single();
            var commentNode = result.comment().Single();

            Assert.That(textNode.GetText(), Is.EqualTo(expectedText));
            Assert.That(commentNode.GetText(), Is.EqualTo(expectedComment));
        }

    }
}
