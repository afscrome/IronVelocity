using IronVelocity.CodeAnalysis.Text;
using NUnit.Framework;

namespace IronVelocity.Tests.CodeAnalysis.Text
{
    public class TextLineTests
    {
        [Test]
        public void When_Creating_TextLine_Positions_Are_Persisted()
        {
            var sourceText = new SourceText("bar");
            var textLine = new TextLine(63, 10, 23, 32);

            Assert.That(textLine.LineNumber, Is.EqualTo(63));
            Assert.That(textLine.Start, Is.EqualTo(10));
            Assert.That(textLine.Length, Is.EqualTo(23));
            Assert.That(textLine.LengthIncludingLineBreak, Is.EqualTo(32));
            Assert.That(textLine.End, Is.EqualTo(10 + 23));
        }

    }
}