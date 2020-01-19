using System.Collections.Immutable;
using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.CodeAnalysis.Tests.Syntax
{
    public class ParseTreePrinterTests
    {
        [Test]
        public void PrintsSingleTokenWithoutValue()
        {
            var input = new SyntaxToken(SyntaxKind.PlusToken, 73, "something");

            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo("PlusToken"));
        }

        [Test]
        public void PrintsSingleTokenWithValue()
        {
            var input = new SyntaxToken(SyntaxKind.BadToken, 73, "foobar", 123);

            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo("BadToken: 123"));
        }

        [Test]
        public void PrintsNodeWithSingleChild()
        {
            var input = new SyntaxNodeWithChildren(
                SyntaxKind.LiteralExpression,
                new SyntaxToken(SyntaxKind.NumberToken, 0, "", 42)
            );

            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo(@"
LiteralExpression
 └─NumberToken: 42".TrimStart()));
        }

        [Test]
        public void PrintsNodeWithMultipleChildren()
        {
            var input = new SyntaxNodeWithChildren(
                SyntaxKind.LiteralExpression,
                new SyntaxToken(SyntaxKind.MinusToken, 0, ""),
                new SyntaxToken(SyntaxKind.NumberToken, 0, "", "72")
            );

            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo(@"
LiteralExpression
 ├─MinusToken
 └─NumberToken: 72".TrimStart()));
        }

        [Test]
        public void PrintsNodeWithNestedChild()
        {
            var input = new SyntaxNodeWithChildren(
                SyntaxKind.LiteralExpression,
                new SyntaxNodeWithChildren(
                    SyntaxKind.LiteralExpression,
                    new SyntaxToken(SyntaxKind.MinusToken, 0, "")
                )
            );

            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo(@"
LiteralExpression
 └─LiteralExpression
    └─MinusToken".TrimStart()));
        }

        [Test]
        public void PrintsTwoChildrenWhenFirstHasChild()
        {
            var input = new SyntaxNodeWithChildren(
                SyntaxKind.LiteralExpression,
                new SyntaxNodeWithChildren(
                    SyntaxKind.LiteralExpression,
                    new SyntaxToken(SyntaxKind.LiteralToken, 0, "")
                ),
                new SyntaxToken(SyntaxKind.SingleLineComment, 0, "")
            );

            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo(@"
LiteralExpression
 ├─LiteralExpression
 │  └─LiteralToken
 └─SingleLineComment".TrimStart()));
        }

        [Test]
        public void PrintsTwoNodesEachWithChildren()
        {
            var input = new SyntaxNodeWithChildren(
                SyntaxKind.LiteralExpression,
                new SyntaxToken(SyntaxKind.SingleLineComment, 0, ""),
                new SyntaxNodeWithChildren(
                    SyntaxKind.LiteralExpression,
                    new SyntaxToken(SyntaxKind.LiteralToken, 0, "")
                )
            );


            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo(@"
LiteralExpression
 ├─SingleLineComment
 └─LiteralExpression
    └─LiteralToken".TrimStart()));
        }


        [Test]
        public void PrintsComplexTree()
        {
            var input = new SyntaxNodeWithChildren(
                SyntaxKind.BinaryExpression,
                new SyntaxNodeWithChildren(
                    SyntaxKind.BinaryExpression,
                    new SyntaxToken(SyntaxKind.NumberToken, 0, "", 7),
                    new SyntaxToken(SyntaxKind.SlashToken, 0, ""),
                    new SyntaxToken(SyntaxKind.NumberToken, 0, "", 3)
                ),
                new SyntaxToken(SyntaxKind.MinusToken, 0, ""),
                new SyntaxNodeWithChildren(
                    SyntaxKind.BinaryExpression,
                    new SyntaxToken(SyntaxKind.NumberToken, 0, "", 2),
                    new SyntaxToken(SyntaxKind.StarToken, 0, ""),
                    new SyntaxToken(SyntaxKind.NumberToken, 0, "", 0)
                )
            );

            var result = ParseTreePrinter.PrettyPrint(input);

            Assert.That(result, Is.EqualTo(@"
BinaryExpression
 ├─BinaryExpression
 │  ├─NumberToken: 7
 │  ├─SlashToken
 │  └─NumberToken: 3
 ├─MinusToken
 └─BinaryExpression
    ├─NumberToken: 2
    ├─StarToken
    └─NumberToken: 0".TrimStart()));
        }

        private class SyntaxNodeWithChildren : SyntaxNode
        {
            private readonly ImmutableArray<SyntaxNode> _children;

            public SyntaxNodeWithChildren(SyntaxKind kind, params SyntaxNode[] children)
            {
                Kind = kind;
                _children = ImmutableArray.Create(children);
            }

            public override SyntaxKind Kind { get; }
            public override ImmutableArray<SyntaxNode> GetChildren() => _children;
        }
    }
}