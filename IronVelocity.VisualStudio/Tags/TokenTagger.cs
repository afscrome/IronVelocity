using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;

namespace IronVelocity.VisualStudio.Tags
{
    internal sealed class TokenTagger : ITagger<TokenTag>
    {
        ITextBuffer _buffer;

        internal TokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<TokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                int curLoc = containingLine.Start.Position;
                string[] tokens = containingLine.GetText().ToLower().Split(' ');

                foreach (string ookToken in tokens)
                {

                    //add an extra char location because of the space
                    curLoc += ookToken.Length + 1;
                }
            }
            yield break;
        }
    }
}
