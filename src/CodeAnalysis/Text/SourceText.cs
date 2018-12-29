
using System;
using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Text
{
    public class SourceText
    {
        private readonly string _text;
        public ImmutableArray<TextLine> Lines { get; }

        public SourceText(string text)
        {
            _text = text;
            Lines = ParseLines(text);
        }

        public int GetLineIndex(int position)
        {
            if (position >= _text.Length)
                throw new ArgumentOutOfRangeException(nameof(position), position, $"Position {position} is longer than the text length {_text.Length}");

            int lowerBound = 0;
            int upperBound = Lines.Length - 1;

            while (lowerBound <= upperBound)
            {
                var middleLineIndex = lowerBound + (upperBound - lowerBound) / 2;
                var start = Lines[middleLineIndex].Start;

                if (position == start)
                    return middleLineIndex;
                else if (start > position)
                    upperBound = middleLineIndex -1;
                else
                    lowerBound = middleLineIndex + 1;                
            }

            return lowerBound - 1;
        }

        private static ImmutableArray<TextLine> ParseLines(string text)
        {
            var textLines = ImmutableArray.CreateBuilder<TextLine>();

            int positionOfFinalCharacter = text.Length - 1;
            int position = 0;
            int lineStart = 0;
            int lineNumber = 1;
            int lineEnd;

            void ReportLine(int lineBreakLength)
            {
                int length = lineEnd - lineStart;
                var line = new TextLine(lineNumber++, lineStart, length, length + lineBreakLength);
                textLines.Add(line);
                lineStart = position + lineBreakLength;
            }

            while (position < text.Length)
            {
                var currentChar = text[position];
                if (currentChar == '\n')
                {
                    lineEnd = position;
                    ReportLine(1);
                }
                else if (currentChar == '\r')
                {
                    lineEnd = position;

                    if (position != positionOfFinalCharacter && text[position + 1] == '\n')
                    {
                        ReportLine(2);
                        position++;
                    }
                    else
                    {
                        ReportLine(1);
                    }
                }
                position++;
            }

            lineEnd = text.Length;
            ReportLine(0);

            return textLines.ToImmutable();

        }
    }
}