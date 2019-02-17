using IronVelocity.CodeAnalysis.Syntax;
using NUnit.Framework;

namespace IronVelocity.Tests.CodeAnalysis.Syntax
{
    public class ParserTests
    {
        [Test]
        public void ParsesNumber()
        {
            var tokens = new SyntaxToken[] {
                new SyntaxToken(SyntaxKind.NumberToken, 0, "1234", 1234)
            };

            var parser = new IronVelocity.CodeAnalysis.Syntax.Parser(tokens);

            var expression = parser.ParseExpression();

            Assert.That(expression, Is.InstanceOf<LiteralExpressionSyntax>());
            var literal = (LiteralExpressionSyntax)expression;
            Assert.That(literal.Value, Is.EqualTo(1234));
            Assert.That(literal.LiteralToken, Is.EqualTo(tokens[0]));
        }
    }
}