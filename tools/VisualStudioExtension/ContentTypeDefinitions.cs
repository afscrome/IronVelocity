using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace IronVelocity.VisualStudio
{
    //[assembly: ProvideLanguageExtension(".vm", guid)]
    internal sealed class ContentTypeDefinitions
    {
        public const string ContentType = "IronVelocity";

        [Export]
        [Name(ContentType)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition VelocityContentType = null;

        [Export]
        [Name("VelocityTemplate")]
        [BaseDefinition("projection")]
        internal static ContentTypeDefinition VelocityTemplateContentType = null;

        [Export]
        [FileExtension(".vm")]
        [ContentType(ContentType)]
        internal static FileExtensionToContentTypeDefinition VmFileType = null;

        [Export]
        [FileExtension(".vmhtml")]
        [ContentType("VelocityTemplate")]
        internal static FileExtensionToContentTypeDefinition VmHtmlFileType = null;
    }
}
