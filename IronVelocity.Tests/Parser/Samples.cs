using IronVelocity.Parser;
using System;
using System.Collections.Generic;

namespace IronVelocity.Tests.Parser
{
    public static class Samples
    {
        public static IReadOnlyCollection<string> GetSamples(SampleType type)
        {
            switch (type)
            {
                case SampleType.ReferenceLikeText:
                    return ReferenceLikeText;
                case SampleType.AmbigiousDelimiters:
                    return AmbigiousDelimiters;
                case SampleType.Reference:
                    return References;
                case SampleType.BlockComment:
                    return BlockComments;
                case SampleType.LineComment:
                    return SingleLineComments;
              default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

        }

        public static Type GetSampleParserNodeType(SampleType type)
        {
            switch (type)
            {
                case SampleType.ReferenceLikeText:
                case SampleType.AmbigiousDelimiters:
                    return typeof(VelocityParser.TextContext);
                case SampleType.Reference:
                    return typeof(VelocityParser.ReferenceContext);
                case SampleType.BlockComment:
                case SampleType.LineComment:
                    return typeof(VelocityParser.CommentContext);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static bool MayBeProblematicToCombine(string left, string right)
        {
            return left.EndsWith("#") && (right.StartsWith("#") || right.StartsWith("{"));
        }

        private static string[] References { get; } = new[]
        {
            "$informal", "$!silent", "${formal}", "$!{formalsilent}",
            "$informal.property", "$!silent.property", "${formal.property}", "$!{formalsilent.property}",
            "$informal.method()", "$!silent.method()", "${formal.method()}", "$!{formalsilent.method()}"
        };

        /// <summary>
        /// Some examples of text that starts like a reference, but isn't as there is no Identifier.
        /// These patterns have historically caused lots of problems 
        /// </summary>
        private static string[] ReferenceLikeText { get; } = new[]
        {
            "$",
            "$123",
            "$$",
            "$!",
            "$!123",
            "$!!",
            "$!$",
            "$.",
            "$.(",
            "$.test",
            "$(",
            // formal versions of the above
            "${",
            "${123",
            "${$",
            "$!{",
            "$!{123",
            "$!{$",
            "${}",
        };

        private static string[] AmbigiousDelimiters { get; } = new[]
        {
            ".",
            ".(",
            "..",
            "..text",
            "$",
            ")",
            "}",
            "{",
            "!",
            "#",
            "]",
            "#}",
        };

        private static string[] SingleLineComments { get; } = new[]
        {
            "##Single\r",
            "##Line\n",
            "##Comment\r\n",
        };

        private static string[] BlockComments { get; } = new[]
        {
            "#*SingleLineBlock*#",
            "#*Multi\rLine\nBlock*#",
            "#*Nested #*Block*# #*Comment*# *#"
        };

    }

    public enum SampleType
    {
        ReferenceLikeText,
        AmbigiousDelimiters,
        Reference,
        BlockComment,
        LineComment
    }
}
