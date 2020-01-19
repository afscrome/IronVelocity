using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.CodeAnalysis.Tests.Syntax
{
    public class SyntaxTokenTests
    {
        [Test]
        public void Persists_Constructor_Inputs()
        {
            var value = new object();
            var token = new SyntaxToken(SyntaxKind.BadToken, 74, "Text", value);

            Assert.That(token.Kind, Is.EqualTo(SyntaxKind.BadToken));
            Assert.That(token.Position, Is.EqualTo(74));
            Assert.That(token.Text, Is.EqualTo("Text"));
            Assert.That(token.Value, Is.EqualTo(value));
        }

        [Test]
        public void Calculates_Span_Correctly()
        {
            var token = new SyntaxToken(SyntaxKind.BadToken, 83, "TextOfLength14");

            Assert.That(token.Span.Start, Is.EqualTo(83));
            Assert.That(token.Span.Length, Is.EqualTo(14));
            Assert.That(token.Span.End, Is.EqualTo(83 + 14));
        }
    }
}
