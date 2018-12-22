using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace IronVelocity.VisualStudio.Tags
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentTypeDefinitions.ContentType)]
    [TagType(typeof(TokenTag))]
    internal sealed class TokenTagProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new TokenTagger(buffer) as ITagger<T>;
        }
    }
}
