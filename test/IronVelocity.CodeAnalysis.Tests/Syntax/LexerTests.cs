using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Tests.Syntax
{
    public class LexerTests
    {
        [Test]
        public void Lexes_EndOfFile_At_End_Of_Input()
            => AssertFirstToken("", SyntaxKind.EndOfFileToken, "");

        [TestCaseSource(nameof(SingleTokens))]
        public void Lexes_Single_Token(SyntaxKind kind, string input)
            => AssertFirstToken(input, kind);

        [TestCaseSource(nameof(GetSimpleTokenCombinations))]
        public void Lexes_Simple_Token_Combination(string leftText, string rightText, SyntaxKind leftKind, SyntaxKind rightKind)
        {
            var input = leftText + rightText;
            var lexer = new Lexer(input);

            var tokens = lexer.ReadAllTokens();

            Assert.That(tokens, Has.Length.EqualTo(2));

            AssertToken(tokens[0], leftKind, 0, leftText);
            AssertToken(tokens[1], rightKind, leftText.Length, rightText);
        }

        [TestCaseSource(nameof(GetComplexTokenCombinations))]
        public void Lexes_Complex_Token_Combination(string leftText, string rightText, SyntaxKind[] expectedKinds)
        {
            var input = leftText + rightText;
            var lexer = new Lexer(input);

            var tokens = lexer.ReadAllTokens();

            var tokenKinds = tokens.Select(x => x.Kind).ToArray();

            Assert.That(tokenKinds, Is.EquivalentTo(expectedKinds));
        }

        private static IEnumerable<object[]> SingleTokens()
        {
            foreach (var kind in TokenSamples.LexerTokenKinds)
            {
                foreach (var sample in TokenSamples.GetSamplesForKind(kind))
                {
                    yield return new object[] { kind, sample };
                }
            }
        }

        private static IEnumerable<object[]> GetSimpleTokenCombinations()
        {
            foreach (var kinds in Helper.GetAllPairs(TokenSamples.LexerTokenKinds))
            {
                if (GetSpecialTokenCombination(kinds.left, kinds.right) != null)
                {
                    continue;
                }

                foreach (var leftSample in TokenSamples.GetSamplesForKind(kinds.left))
                {
                    foreach (var rightSample in TokenSamples.GetSamplesForKind(kinds.right))
                    {
                        yield return new object[] { leftSample, rightSample, kinds.left, kinds.right};
                    }
                }
            }
        }

        private static IEnumerable<object[]> GetComplexTokenCombinations()
        {
            foreach (var kinds in Helper.GetAllPairs(TokenSamples.LexerTokenKinds))
            {
                var specialCombination = GetSpecialTokenCombination(kinds.left, kinds.right);
                if (specialCombination == null)
                {
                    continue;
                }

                foreach (var leftSample in TokenSamples.GetSamplesForKind(kinds.left))
                {
                    foreach (var rightSample in TokenSamples.GetSamplesForKind(kinds.right))
                    {
                        // Block comments after a # are complicated as if the comment contains a newline, the bit
                        // after the new line is no longer inside a comment.
                        if ((kinds.left == SyntaxKind.HashToken || kinds.left == SyntaxKind.SingleLineComment)
                            && kinds.right == SyntaxKind.BlockComment
                            && (rightSample.Contains("\r") || rightSample.Contains("\n")))
                        {
                            continue;
                        }


                        yield return new object[] { leftSample, rightSample, specialCombination };
                    }
                }
            }
        }

        private static SyntaxKind[] GetSpecialTokenCombination(SyntaxKind left, SyntaxKind right)
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
                    case SyntaxKind.IdentifierToken:
                        return new[] { left };
                }
            }

            switch (left)
            {
                //If the start is a single line comment, anything that isn't a new line becomes part of the comment
                case SyntaxKind.SingleLineComment when right != SyntaxKind.VerticalWhitespaceToken:
                    return new[] { SyntaxKind.SingleLineComment };

                //If the left is a hash, any token on the right starting with a hash or star will become some form of comment
                case SyntaxKind.HashToken:
                    if (right == SyntaxKind.SingleLineComment
                        || right == SyntaxKind.BlockComment
                        || right == SyntaxKind.LiteralToken
                        || right == SyntaxKind.HashToken)
                    {
                        return new[] { SyntaxKind.SingleLineComment };
                    }
                    else if (right == SyntaxKind.StarToken)
                    {
                        //This becomes the start of a block comment, but without hte end.
                        return new[] { SyntaxKind.BadToken };
                    };
                    break;

                // Two keywords / identifiers will combine to a single large identifier
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.IdentifierToken:
                    if (right == SyntaxKind.TrueKeyword || right == SyntaxKind.FalseKeyword || right == SyntaxKind.IdentifierToken)
                    {
                        return new[] { SyntaxKind.IdentifierToken };
                    }
                    break;

                case SyntaxKind.BangToken when right == SyntaxKind.EqualsEqualsToken:
                    return new[] { SyntaxKind.BangEqualsToken, SyntaxKind.BadToken };
            }

            return null;
        }

        private static void AssertFirstToken(string input, SyntaxKind expectedKind) => AssertFirstToken(input, expectedKind, input);
        private static void AssertFirstToken(string input, SyntaxKind expectedKind, string expectedTokenText, object? expectedValue = null)
        {
            var lexer = new Lexer(input);

            var token = lexer.NextToken();

            AssertToken(token, expectedKind, 0, expectedTokenText, expectedValue);
        }

        private static void AssertToken(SyntaxToken token, SyntaxKind expectedKind, int expectedPosition, string expectedTokenText, object? expectedValue = null)
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
