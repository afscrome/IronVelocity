using IronVelocity.VisualStudio.Tags;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace IronVelocity.VisualStudio
{
    internal sealed class Classifier : ITagger<ClassificationTag>
    {
        private readonly ITagAggregator<TokenTag> _aggregator;
        private readonly IStandardClassificationService _standardClassifications;

        internal Classifier(ITagAggregator<TokenTag> tagAggregator, IStandardClassificationService standardClassifications)
        {
            _aggregator = tagAggregator;
            _standardClassifications = standardClassifications;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var globalSpan = spans.FirstOrDefault();

            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                IClassificationType classification;
                switch (tagSpan.Tag.Type)
                {
                    case TokenType.Literal:
                        classification = _standardClassifications.Literal;
                        break;
                    case TokenType.Operator:
                        classification = _standardClassifications.Operator;
                        break;
                    case TokenType.Keyword:
                        classification = _standardClassifications.Keyword;
                        break;
                    case TokenType.Comment:
                        classification = _standardClassifications.Comment;
                        break;
                    case TokenType.Identifier:
                        classification = _standardClassifications.Identifier;
                        break;
                    case TokenType.StringLiteral:
                        classification = _standardClassifications.StringLiteral;
                        break;
                    case TokenType.NumberLiteral:
                        classification = _standardClassifications.NumberLiteral;
                        break;
                    case TokenType.SymbolReference:
                        classification = _standardClassifications.SymbolReference;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                var span = tagSpan.Span.GetSpans(globalSpan.Snapshot)[0];
                yield return new TagSpan<ClassificationTag>(span, new ClassificationTag(classification));
            }
            yield break;
        }
    }


}
