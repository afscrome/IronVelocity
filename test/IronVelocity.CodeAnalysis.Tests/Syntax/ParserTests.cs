using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using static IronVelocity.Tests.CodeAnalysis.Syntax.SyntaxTreeAssertions;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public partial class ParserTests
    {
        [Test]
        public void Parses_NumberLiteral()
        {
            AssertParseTree("1234", Literal(1234));
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void Parses_BooleanLiteral(string text, object expectedValue)
        {
            AssertParseTree(text, Literal(expectedValue));
        }

        [TestCaseSource(typeof(SyntaxFacts), nameof(SyntaxFacts.GetUnaryOperatorKinds))]
        public void Parses_AllUnaryExpressionOperators(SyntaxKind kind)
        {
            var text = $"{SyntaxFacts.GetText(kind)}0";

            AssertParseTree(text,
                Unary(
                    kind,
                    Literal(0)
                )
            );
        }

        [TestCaseSource(typeof(SyntaxFacts), nameof(SyntaxFacts.GetBinaryOperatorKinds))]
        public void Parses_AllBinaryExpressionOperators(SyntaxKind kind)
        {
            var text = $"true {SyntaxFacts.GetText(kind)} false";

            AssertParseTree(text,
                Binary(
                    Literal(true),
                    kind,
                    Literal(false)
                )
            );
        }

        [TestCaseSource(nameof(BinaryOperatorPairs))]
        public void Parses_MultipleBinaryExpression_HonouringPrecedence(SyntaxKind left, SyntaxKind right)
        {
            var leftPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(left);
            var rightPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(right);

            var leftText = SyntaxFacts.GetText(left);
            var rightText = SyntaxFacts.GetText(right);

            var text = $"1 {leftText} 2 {rightText} 3";

            if (rightPrecedence > leftPrecedence)
            {
                AssertParseTree(text,
                    Binary(
                        Literal(1),
                        left,
                        Binary(
                            Literal(2),
                            right,
                            Literal(3)
                        )

                    )
                );
            }
            else
            {
                AssertParseTree(text,
                    Binary(
                        Binary(
                            Literal(1),
                            left,
                            Literal(2)
                        ),
                        right,
                        Literal(3)
                    )
                );
            }
        }


        private static IEnumerable<object[]> BinaryOperatorPairs()
            => GetAllPairs(SyntaxFacts.GetBinaryOperatorKinds());
        private static IEnumerable<object[]> UnaryOperatorPairs()
            => GetAllPairs(SyntaxFacts.GetUnaryOperatorKinds());

        public static IEnumerable<object[]> GetAllPairs<T>(IEnumerable<T> values)
        {
            return from left in values
                   from right in values
                   select new object[] { left, right };
        }

    }
}