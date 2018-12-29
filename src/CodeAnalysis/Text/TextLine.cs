using System.Diagnostics;

namespace IronVelocity.CodeAnalysis.Text
{
    [DebuggerDisplay("{Start}:{Length}({LengthIncludingLineBreak}) {Text}")]
    public class TextLine
    {
        public TextLine(int lineNumber, int start, int length, int lineBreakLength)
        {
            LineNumber = lineNumber;
            Start = start;
            Length = length;
            LengthIncludingLineBreak = lineBreakLength;
        }

        public int LineNumber { get; }
        public int Start { get; }
        public int Length { get; }
        public int LengthIncludingLineBreak { get; }
        public int End => Start + Length;
    }
}