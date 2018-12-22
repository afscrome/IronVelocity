using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace IronVelocity.VisualStudio.IntelliSense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(ContentTypeDefinitions.ContentType)]
    [Name("Velocity Completion")]
    public class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal IGlyphService GlyphService {get;set;}

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new CompletionSource(textBuffer, NavigatorService, GlyphService);
        }
    }
}
