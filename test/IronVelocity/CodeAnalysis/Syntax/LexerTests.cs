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
            var tokenKinds = Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().ToArray();

            foreach (var kind in tokenKinds)
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
            var tokenKinds = Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().ToArray();

            foreach (var leftKind in tokenKinds)
            {
                foreach (var rightKind in tokenKinds)
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
            //Two numbers in a row are a single larger number
            if (left == SyntaxKind.NumberToken && right == SyntaxKind.NumberToken)
            {
                return false;
            }

            //If the start is a single line comment, anything that isn't a new line becomes part of the comment
            if (left == SyntaxKind.SingleLineComment)
            {
                return right == SyntaxKind.VerticalWhitespaceToken;
            }

            //If the left is a hash, any token on the right starting with a hash  or star will become a comment
            if (left == SyntaxKind.HashToken)
            {
                return right != SyntaxKind.SingleLineComment
                    && right != SyntaxKind.BlockComment
                    && right != SyntaxKind.LiteralToken
                    && right != SyntaxKind.HashToken
                    && right != SyntaxKind.StarToken;
            }

            //If left and right are the same kind, concatenating the input will
            //sometimes result in one bigger token, rather than two seperate ones
            if (left == right)
            {
                switch(left)
                {
                    case SyntaxKind.HorizontalWhitespaceToken:
                    case SyntaxKind.VerticalWhitespaceToken:
                    case SyntaxKind.SingleLineComment:
                        return false;
                }
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
            Assert.That(token.Value, Is.EqualTo(expectedValue));
        }

    }
}
