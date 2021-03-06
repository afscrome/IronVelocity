﻿using IronVelocity.VisualStudio.Tags;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace IronVelocity.VisualStudio
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentTypeDefinitions.ContentType)]
    [TagType(typeof(ClassificationTag))]
    internal sealed class ClassifierProvider : ITaggerProvider
    {
        [Import]
        internal IStandardClassificationService StandardClassificationService = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<TokenTag> ookTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<TokenTag>(buffer);

            return new Classifier(ookTagAggregator, StandardClassificationService) as ITagger<T>;
        }
    }
}
