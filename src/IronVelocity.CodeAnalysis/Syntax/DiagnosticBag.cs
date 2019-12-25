using IronVelocity.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class DiagnosticBag
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IImmutableList<Diagnostic> Diagnostics => _diagnostics.ToImmutableList();

        private void Report(TextSpan span, string message)
        {
            _diagnostics.Add(new Diagnostic(span, message));
        }

        public void ReportBadCharacter(int position, char character)
            => Report(new TextSpan(position, 1), $"Unexpected Character: '{character}'");

        public void ReportInvalidNumber(int position, string text)
            => Report(new TextSpan(position, text.Length), $"Could not parse '{text}' as a number");

        public void ReportUnexpectedToken(SyntaxToken token, params SyntaxKind[] expectedTokens)
        {
            var expectedTokenString = string.Join(", ", expectedTokens.Select(x => "<" + x + ">"));
            Report(token.Span, $"Unexpected Token <{token.Kind}> - expected {expectedTokenString}");
        }


        public void ReportUndefinedUnaryOperator(SyntaxToken operatorToken, Type type)
            => Report(operatorToken.Span, $"Unary operator '{operatorToken.Text}' not defiend for type {type}");

        public void ReportUndefinedBinaryOperator(SyntaxToken operatorToken, Type leftType, Type rightType)
            => Report(operatorToken.Span, $"Binary operator '{operatorToken.Text}' not defiend between types {leftType} and {rightType}");


    }


}
