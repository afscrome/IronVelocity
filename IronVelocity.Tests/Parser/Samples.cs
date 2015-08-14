using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Parser
{
    public static class Samples
    {
        public static IEnumerable<string> References { get; } = new[]
        {
            "$informal", "$!silent", "${formal}", "$!{formalsilent}",
            "$informal.property", "$!silent.property", "${formal.property}", "$!{formalsilent.property}",
            "$informal.method()", "$!silent.method()", "${formal.method()}", "$!{formalsilent.method()}"
        };

        /// <summary>
        /// Some examples of text that starts like a reference, but isn't as there is no IDENTIFIER.
        /// These patterns have historically caused lots of problems 
        /// </summary>
        public static IEnumerable<string> ReferenceLikeText { get; } = new[]
        {
            "$",
            "$123",
            "$$",
            "$!",
            "$!123",
            "$!!",
            "$!$",
            // formal versions of the above
            "${",
            "${123",
            "${$",
            "$!{",
            "$!{123",
            "$!{$",
            "${}",
        };

        public static IEnumerable<string> PotentiallyAmbigiousReferenceDelimiters { get; } = new[]
        {
            ".",
            "..",
            "$",
            ")",
            "}",
            "{",
            "!",
            "#",
            "]"
        };

        public static IEnumerable<string> Comments { get; } = new[]
        {
            "##Single\r",
            "##Line\n",
            "##Comment\r\n",
            "#*SingleLineBlock*#",
            "#*Multi\rLine\nBlock*#",
            "#*Nested #*Block*# #*Comment*# *#"
        };

    }
}
