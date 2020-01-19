using IronVelocity.CodeAnalysis.Text;
using NUnit.Framework;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Tests.Text
{
    public class SourceTextTests
    {
        [Test]
        public void First_Line_Is_Line_One()
        {
            var sourceText = new SourceText("FirstLine");

            Assert.That(sourceText.Lines, Has.Length.EqualTo(1));
            Assert.That(sourceText.Lines[0].LineNumber, Is.EqualTo(1));
        }

        [Test]
        public void Reads_Empty_Line_As_Line_With_Empty_Text()
        {
            var sourceText = new SourceText("");

            Assert.That(sourceText.Lines, Has.Length.EqualTo(1));
            AssertLine(sourceText.Lines[0], 1, 0, "");
        }

        [Test]
        public void Reads_Single_Line()
        {
            var sourceText = new SourceText("SomeLine");

            Assert.That(sourceText.Lines, Has.Length.EqualTo(1));
            AssertLine(sourceText.Lines[0], 1, 0, "SomeLine");
        }

        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("\r", "\r")]
        [TestCase("\n", "\n")]
        [TestCase("\r\n", "\r\n")]
        [TestCase("\r", "\r\n")]
        [TestCase("\r\n", "\n")]
        [TestCase("\r", "\r", "\r")]
        [TestCase("\n", "\n", "\n")]
        [TestCase("\r\n", "\r\n", "\r\n")]
        public void Reads_Empty_Lines_Correctly(params string[] lines)
        {
            var input = string.Concat(lines);
            var sourceText = new SourceText(input);

            Assert.That(sourceText.Lines, Has.Length.EqualTo(lines.Length + 1));

            int start = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var newline = lines[i];

                AssertLine(sourceText.Lines[i], i + 1, start, "", newline);
                start += newline.Length;
            }

            AssertLine(sourceText.Lines.Last(), lines.Length + 1, input.Length, "", "");
        }

        [TestCase(0, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 1)]
        [TestCase(8, 1)]
        [TestCase(9, 2)]
        [TestCase(13, 2)]
        public void Calculates_LineIndex_Correctly(int position, int expectedLine)
        {
            var input = "One\nTwo\r\nThree";

            var sourceText = new SourceText(input);

            Assert.That(sourceText.GetLineIndex(position), Is.EqualTo(expectedLine));
        }

        private void AssertLine(TextLine line, int expectedLineNumber, int expectedStart, string expectedText, string expectedLineBreak = "")
        {
            int expectedLength = expectedText.Length;
            int expectedLengthIncludingLineBreak = expectedLength + expectedLineBreak.Length;

            Assert.That(line.LineNumber, Is.EqualTo(expectedLineNumber));
            Assert.That(line.Start, Is.EqualTo(expectedStart));
            Assert.That(line.Length, Is.EqualTo(expectedLength));
            Assert.That(line.LengthIncludingLineBreak, Is.EqualTo(expectedLengthIncludingLineBreak));
            Assert.That(line.End, Is.EqualTo(expectedStart + expectedLength));
        }
    }
}