using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Tests.Syntax
{
    public class SyntaxFactsTests
    {
        [TestCaseSource(nameof(SyntaxKindsWithText))]
        public void SyntaxKindTextMatchedByLexer(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);

            var tokens = SyntaxTree.ParseTokens(text!);
            var token = tokens.Where(x => x.Kind != SyntaxKind.EndOfFileToken).Single();

            Assert.That(token.Kind, Is.EqualTo(kind));
            Assert.That(token.Text, Is.EqualTo(text));
        }

        private static IEnumerable<SyntaxKind> SyntaxKindsWithText
            => ((SyntaxKind[])Enum.GetValues(typeof(SyntaxKind)))
                .Where(x => SyntaxFacts.GetText(x) != null);


    }
}
