using IronVelocity.VisualStudio.Tags;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace IronVelocity.VisualStudio
{
    internal sealed class Classifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<TokenTag> _aggregator;

        internal Classifier(ITextBuffer buffer, ITagAggregator<TokenTag> ookTagAggregator, IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = ookTagAggregator;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {

            }
            yield break;
        }
    }


}
