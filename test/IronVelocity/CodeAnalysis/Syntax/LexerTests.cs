using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        public class IndividualTokens
        {
            [Test]
            public void Lexes_EndOfFile_At_End_Of_Input()
                => AssertFirstToken("", SyntaxKind.EndOfFile, "\0");

            [TestCaseSource(typeof(TokenSamples), nameof(TokenSamples.BadToken))]
            public void Lexes_BadToken(string input)
                => AssertFirstToken("?", SyntaxKind.BadToken);

            [TestCaseSource(typeof(TokenSamples), nameof(TokenSamples.SingleLineComment))]
            public void Lexes_Single_Line_Comment(string input)
                => AssertFirstToken(input, SyntaxKind.SingleLineComment);

            [TestCaseSource(typeof(TokenSamples), nameof(TokenSamples.BlockComment))]
            public void Lexes_Block_Comment(string input)
                => AssertFirstToken(input, SyntaxKind.BlockComment);

            [TestCaseSource(typeof(TokenSamples), nameof(TokenSamples.Literal))]
            public void Lexes_Literal(string input)
                => AssertFirstToken(input, SyntaxKind.Literal);

            [TestCaseSource(typeof(TokenSamples), nameof(TokenSamples.HorizontalWhitespace))]
            public void Lexes_Horizontal_Whitespace(string input)
                => AssertFirstToken(input, SyntaxKind.HorizontalWhitespace);

            [TestCaseSource(typeof(TokenSamples), nameof(TokenSamples.VerticalWhitespace))]
            public void Lexes_Vertical_Whitespace(string input)
                => AssertFirstToken(input, SyntaxKind.VerticalWhitespace);
        }

        public class TokenCombinations
        {
            [TestCaseSource(nameof(GetTokenCombinations))]
            public void Lexes_Token_Combination(SyntaxKind leftKind, SyntaxKind rightKind, string leftText, string rightText)
            {
                var input = leftText + rightText;
                var lexer = new Lexer(input);

                var firstToken = lexer.NextToken();
                AssertToken(firstToken, leftKind, 0, leftText);

                var secondToken = lexer.NextToken();
                AssertToken(secondToken, rightKind, leftText.Length, rightText);
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
                //Nothing can come after end of file
                if (left == SyntaxKind.EndOfFile)
                {
                    return false;
                }

                //If the start is a single line comment, anything that isn't a new line becomes part of the comment
                if (left == SyntaxKind.SingleLineComment)
                {
                    return right != SyntaxKind.HorizontalWhitespace;
                }

                //If left and right are the same kind, concatenating the input will
                //sometimes result in one bigger token, rather than two seperate ones
                if (left == right)
                {
                    switch(left)
                    {
                        case SyntaxKind.HorizontalWhitespace:
                        case SyntaxKind.VerticalWhitespace:
                        case SyntaxKind.SingleLineComment:
                            return false;
                    }
                }


                return true;
            }
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
