using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Test]
        public void Lexes_EndOfFile_At_End_Of_Input()
            => AssertFirstToken("", SyntaxKind.EndOfFileToken, "\0");

        [TestCaseSource(nameof(SingleTokens))]
        public void Lexes_Single_Token(SyntaxKind kind, string input)
            => AssertFirstToken(input, kind);

        [TestCaseSource(nameof(GetTokenCombinations))]
        public void Lexes_Token_Combination(SyntaxKind leftKind, SyntaxKind rightKind, string leftText, string rightText)
        {
            var input = leftText + rightText;
            var lexer = new Lexer(input);

            var tokens = new List<SyntaxToken>();

            while(true)
            {
                var token = lexer.NextToken();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;

                tokens.Add(token);               
            }

            Assert.That(tokens, Has.Count.EqualTo(2));

            AssertToken(tokens[0], leftKind, 0, leftText);
            AssertToken(tokens[1], rightKind, leftText.Length, rightText);
        }

        private static IEnumerable<object[]> SingleTokens()
        {
            var results = new List<object[]>();

            foreach (var kind in TokenSamples.LexerTokenKinds)
            {
                foreach (var sample in TokenSamples.GetSamplesForKind(kind))
                {
                    yield return new object[] { kind, sample };
                }
            }
        }

        private static IEnumerable<object[]> GetTokenCombinations()
        {
            var results = new List<object[]>();

            foreach (var leftKind in TokenSamples.LexerTokenKinds)
            {
                foreach (var rightKind in TokenSamples.LexerTokenKinds)
                {
                    if (!CanCombineTokenTypes(leftKind, rightKind))
                    {
                        continue;
                    }

                    foreach (var leftSample in TokenSamples.GetSamplesForKind(leftKind))
                    {
                        foreach (var rightSample in TokenSamples.GetSamplesForKind(rightKind))
                        {
                            results.Add(new object[] { leftKind, rightKind, leftSample, rightSample });
                        }
                    }

                }
            }
            return results;
        }

        private static bool CanCombineTokenTypes(SyntaxKind left, SyntaxKind right)
        {
            //If left and right are the same kind, concatenating the input will
            //often result in one bigger token, rather than two seperate ones
            if (left == right)
            {
                switch (left)
                {
                    case SyntaxKind.NumberToken:
                    case SyntaxKind.HorizontalWhitespaceToken:
                    case SyntaxKind.VerticalWhitespaceToken:
                    case SyntaxKind.SingleLineComment:
                    case SyntaxKind.TrueKeyword:
                    case SyntaxKind.FalseKeyword:
                    case SyntaxKind.IdentifierToken:
                        return false;
                }
            }

            switch (left)
            {
                //If the start is a single line comment, anything that isn't a new line becomes part of the comment
                case SyntaxKind.SingleLineComment when right != SyntaxKind.VerticalWhitespaceToken:
                    return false;

                //If the left is a hash, any token on the right starting with a hash or star will become some form of comment
                case SyntaxKind.HashToken:
                    if (right == SyntaxKind.SingleLineComment
                        || right == SyntaxKind.BlockComment
                        || right == SyntaxKind.LiteralToken
                        || right == SyntaxKind.HashToken
                        || right == SyntaxKind.StarToken)
                    {
                        return false;
                    };
                    break;

                // Two keywords / identifiers will combine to a single large identifier
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.IdentifierToken:
                    if (right == SyntaxKind.TrueKeyword || right == SyntaxKind.FalseKeyword || right == SyntaxKind.IdentifierToken)
                        return false;
                    break;

            }

            return true;
        }

        private static void AssertFirstToken(string input, SyntaxKind expectedKind) => AssertFirstToken(input, expectedKind, input);
        private static void AssertFirstToken(string input, SyntaxKind expectedKind, string expectedTokenText, object expectedValue = null)
        {
            var lexer = new Lexer(input);

            var token = lexer.NextToken();

            AssertToken(token, expectedKind, 0, expectedTokenText, expectedValue);
        }

        private static void AssertToken(SyntaxToken token, SyntaxKind expectedKind, int expectedPosition, string expectedTokenText, object expectedValue = null)
        {
            Assert.That(token.Kind, Is.EqualTo(expectedKind));
            Assert.That(token.Position, Is.EqualTo(expectedPosition));
            Assert.That(token.Text, Is.EqualTo(expectedTokenText));
            if (expectedValue != null)
            {
                Assert.That(token.Value, Is.EqualTo(expectedValue));
            }
        }

    }
}
