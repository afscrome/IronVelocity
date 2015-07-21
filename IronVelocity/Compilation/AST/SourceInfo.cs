using System;
using System.Diagnostics;

namespace IronVelocity.Compilation.AST
{
    [DebuggerDisplay("{StartLine},{EndLine} : {StartColumn},{EndColumn}")]
    public class SourceInfo
    {
        public SourceInfo(int startLine, int startColumn, int endLine, int endColumn)
        {
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public int StartLine { get; }
        public int StartColumn { get; }
        public int EndLine { get; }
        public int EndColumn { get; }

        public override bool Equals(object obj)
        {
            var sourceInfo = obj as SourceInfo;
            return sourceInfo == null
                ? false
                : sourceInfo == this;
        }

        public static bool operator ==(SourceInfo left, SourceInfo right)
        {
            if (Object.Equals(left, null) || Object.ReferenceEquals(right, null))
                return Object.ReferenceEquals(left, right);
            else
                return left.StartLine == right.StartLine
                && left.StartColumn == right.StartColumn
                && left.EndLine == right.EndLine
                && left.EndColumn == right.EndColumn;

        }

        public static bool operator !=(SourceInfo left, SourceInfo right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash *= 23 + StartLine.GetHashCode();
                hash *= 23 + StartColumn.GetHashCode();
                hash *= 23 + EndLine.GetHashCode();
                hash *= 23 + EndColumn.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return String.Format("{0},{1} : {2},{3}", StartLine, EndLine, StartColumn, EndColumn);
        }

    }
}
