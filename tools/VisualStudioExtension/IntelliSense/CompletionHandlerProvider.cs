using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace IronVelocity.VisualStudio.IntelliSense
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Velocity Completion Handler")]
    [ContentType(ContentTypeDefinitions.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CompletionHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Func<CompletionCommandHandler> createCommandHandler = () =>
            {
                return new CompletionCommandHandler(textViewAdapter, textView, CompletionBroker, ServiceProvider);
            };

            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }
}
