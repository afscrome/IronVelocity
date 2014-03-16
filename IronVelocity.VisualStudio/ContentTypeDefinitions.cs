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
        [BaseDefinition("htmlx")]
        internal static ContentTypeDefinition VelocityContentType = null;

        [Export]
        [FileExtension(".vm")]
        [ContentType(ContentType)]
        internal static FileExtensionToContentTypeDefinition VmFileType = null;
    }
}
