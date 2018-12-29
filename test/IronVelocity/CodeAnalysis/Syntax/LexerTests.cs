using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Test]
        public void Lexes_BadToken_For_Unrecognised_Character()
            => AssertFirstToken("?", SyntaxKind.BadToken);

        [Test]
        public void Lexes_EndOfFile_At_End_Of_Input()
            => AssertFirstToken("", SyntaxKind.EndOfFile, "\0");

        [TestCase("##")]
        [TestCase("##foo")]
        [TestCase("##foo\r", "##foo")]
        public void Lexes_Single_Line_Comment(string input, string expectedText = null)
        {
            expectedText = expectedText ?? input;

            AssertFirstToken(input, SyntaxKind.Comment, expectedText);
        }


        [TestCase("#*Hello World*#")]
        [TestCase("#**#")]
        [TestCase("#***#")]
        [TestCase("#*#*#")]
        [TestCase("#**Hello \r\n \r \n World**#")]
        public void Lexes_Block_Comment(string input)
            => AssertFirstToken(input, SyntaxKind.Comment);

        [TestCase("#[[]]")]
        [TestCase("#[[Hello World]]")]
        [TestCase("#[[$]]")]
        [TestCase("#[[#]]")]
        [TestCase(@"#[[\]]")]
        public void Lexes_Literal(string input)
            => AssertFirstToken(input, SyntaxKind.Literal);


        private static void AssertFirstToken(string input, SyntaxKind kind) => AssertFirstToken(input, kind, input);
        private static void AssertFirstToken(string input, SyntaxKind kind, string expectedTokenText, object value = null)
        {
            var lexer = new Lexer(input);

            var token = lexer.NextToken();

            Assert.That(token.Kind, Is.EqualTo(kind));
            Assert.That(token.Position, Is.EqualTo(0));
            Assert.That(token.Text, Is.EqualTo(expectedTokenText));
            Assert.That(token.Value, Is.EqualTo(value));
        }

    }
}
