using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Test]
        public void Returns_BadToken_For_Unrecognised_Character()
        {
            var input = "?";
            var lexer = new Lexer(input);

            var token = lexer.NextToken();

            Assert.That(token.Kind, Is.EqualTo(SyntaxKind.BadToken));
            Assert.That(token.Position, Is.EqualTo(0));
            Assert.That(token.Text, Is.EqualTo(input));
        }

        [Test]
        public void Returns_EndOfFile_At_End_Of_Input()
        {
            var lexer = new Lexer("");

            var token = lexer.NextToken();

            Assert.That(token.Kind, Is.EqualTo(SyntaxKind.EndOfFile));
            Assert.That(token.Position, Is.EqualTo(0));
            Assert.That(token.Text, Is.EqualTo("\0"));
        }
    }
}
