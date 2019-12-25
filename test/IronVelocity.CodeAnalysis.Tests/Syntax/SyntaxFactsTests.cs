using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronVelocity.CodeAnalysis.Tests.Syntax
{
    public class SyntaxFactsTests
    {
        [TestCaseSource(nameof(SyntaxKindsWithText))]
        public void SyntaxKindTextMatchedByLexer(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);

            var lexer = new Lexer(text);
            var token = lexer.NextToken();

            Assert.That(token.Kind, Is.EqualTo(kind));
            Assert.That(token.Text, Is.EqualTo(text));
        }

        private static IEnumerable<SyntaxKind> SyntaxKindsWithText
            => ((SyntaxKind[])Enum.GetValues(typeof(SyntaxKind)))
                .Where(x => SyntaxFacts.GetText(x) != null);


    }
}
