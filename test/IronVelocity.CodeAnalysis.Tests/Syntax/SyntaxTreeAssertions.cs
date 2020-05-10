using IronVelocity.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Tests.Syntax
{
    public class SyntaxTreeAssertions
    {
        public static void AssertParseTree(string text, SyntaxTreeAssertions assertions)
        {
            var syntaxTree = SyntaxTree.Parse(text);

            AssertParseTree(syntaxTree, assertions);
        }

        public static void AssertParseTree(SyntaxTree syntaxTree, SyntaxTreeAssertions assertions)
        {
            Assert.That(syntaxTree.Diagnostics, Is.Empty);
            assertions.AssertNode(syntaxTree.Root.Expression);
        }

        public static SyntaxTreeAssertions Literal(object value)
        {
            return new SyntaxTreeAssertions()
                .AssertKind(SyntaxKind.LiteralExpression)
                .AssertType<LiteralExpressionSyntax>()
                .AddAssertion<LiteralExpressionSyntax>(x => Assert.That(x.Value, Is.EqualTo(value)));
        }

        public static SyntaxTreeAssertions Binary(SyntaxTreeAssertions left, SyntaxKind op, SyntaxTreeAssertions right)
        {
            return new SyntaxTreeAssertions()
                .AssertKind(SyntaxKind.BinaryExpression)
                .AssertType<BinaryExpressionSyntax>()
                .WithChildAssertions(
                    left,
                    new SyntaxTreeAssertions()
                        .AssertKind(op)
                        .AssertType<SyntaxToken>(),
                    right
                );
        }

        public static SyntaxTreeAssertions Unary(SyntaxKind op, SyntaxTreeAssertions operand)
        {
            return new SyntaxTreeAssertions()
                .AssertKind(SyntaxKind.UnaryExpression)
                .AssertType<UnaryExpressionSyntax>()
                .WithChildAssertions(
                    new SyntaxTreeAssertions()
                        .AssertKind(op)
                        .AssertType<SyntaxToken>(),
                    operand
                );
        }

        private readonly List<Action<SyntaxNode>> _assertions = new List<Action<SyntaxNode>>();
        private readonly List<SyntaxTreeAssertions> _childAssertions = new List<SyntaxTreeAssertions>();


        private SyntaxTreeAssertions AssertType<T>()
            => AddAssertion(x => Assert.That(x, Is.TypeOf<T>()));

        private SyntaxTreeAssertions AssertKind(SyntaxKind kind)
            => AddAssertion(x => Assert.That(x.Kind, Is.EqualTo(kind)));

        private SyntaxTreeAssertions AddAssertion<T>(Action<T> assertion)
            where T : SyntaxNode
        {
            return AssertType<T>().
                AddAssertion(x =>
                {
                    if (x is T castNode)
                    {
                        assertion(castNode);
                    }
                });
        }

        private SyntaxTreeAssertions AddAssertion(Action<SyntaxNode> assertion)
        {
            _assertions.Add(assertion);
            return this;
        }

        public SyntaxTreeAssertions WithChildAssertions(params SyntaxTreeAssertions[] childAssertions)
        {
            _childAssertions.Clear();
            _childAssertions.AddRange(childAssertions);
            return this;
        }

        public void AssertNode(SyntaxNode node)
        {
            foreach (var assetion in _assertions)
            {
                assetion(node);
            }

            if (!_childAssertions.Any())
            {
                return;
            }

            var childNodes = node.GetChildren();
            if (childNodes.Length == _childAssertions.Count)
            {
                for (int i = 0; i < childNodes.Length; i++)
                {
                    _childAssertions[i].AssertNode(childNodes[i]);
                }
            }
            else
            {
                Assert.That(_childAssertions.Count, Is.EqualTo(childNodes.Length));
            }
        }


    }
}