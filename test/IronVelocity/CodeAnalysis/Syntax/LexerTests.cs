using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Test]
        public void Lexes_BadToken_For_Unrecognised_Character()
        {
            var input = "?";
            var lexer = new Lexer(input);

            var token = lexer.NextToken();

            Assert.That(token.Kind, Is.EqualTo(SyntaxKind.BadToken));
            Assert.That(token.Position, Is.EqualTo(0));
            Assert.That(token.Text, Is.EqualTo(input));
        }

        [Test]
        public void Lexes_EndOfFile_At_End_Of_Input()
        {
            var lexer = new Lexer("");

            var token = lexer.NextToken();

            Assert.That(token.Kind, Is.EqualTo(SyntaxKind.EndOfFile));
            Assert.That(token.Position, Is.EqualTo(0));
            Assert.That(token.Text, Is.EqualTo("\0"));
        }

        public class Comments
        {
            [Test]
            public void LexesSingleLineCommentWithNoText()
                => LexComment("##", "##");

            [Test]
            public void LexesSingleLineCommentAtEndOfFile()
                => LexComment("##foo", "##foo");

            [Test]
            public void LexesSingleLineCommentWithNewLineAfter()
                => LexComment("##foo\r", "##foo");

            [Test]
            public void LexesBlockCommentWithNoText()
                => LexComment("#**#", "#**#");

            [Test]
            public void LexesBlockCommentWithText()
                => LexComment("#*Hello World*#", "#*Hello World*#");

            [Test]
            public void LexesBlockCommentWithNewLines()
                => LexComment("#*Hello \r\n \r \n World*#", "#*Hello \r\n \r \n World*#");


            private void LexComment(string input, string expectedText)
            {
                var lexer = new Lexer(input);

                var token = lexer.NextToken();

                Assert.That(token.Kind, Is.EqualTo(SyntaxKind.Comment));
                Assert.That(token.Position, Is.EqualTo(0));
                Assert.That(token.Text, Is.EqualTo(expectedText));
                Assert.Null(token.Value);
            }
        }

    }
}
