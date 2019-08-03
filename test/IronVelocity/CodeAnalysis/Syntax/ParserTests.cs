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
                Number(1234)
            };

            var parser = new IronVelocity.CodeAnalysis.Syntax.Parser(tokens);

            var expression = parser.ParseExpression();

            Assert.That(expression, Is.InstanceOf<LiteralExpressionSyntax>());
            var literal = (LiteralExpressionSyntax)expression;
            Assert.That(literal.Value, Is.EqualTo(1234));
            Assert.That(literal.LiteralToken, Is.EqualTo(tokens[0]));
        }

        [Test]
        public void ParsesBasicBinaryExpression()
        {
            var tokens = new SyntaxToken[]
            {
                Number(45),
                new SyntaxToken(SyntaxKind.PlusToken, 0, "+"),
                Number(72),
            };

            var parser = new IronVelocity.CodeAnalysis.Syntax.Parser(tokens);

            var expression = parser.ParseExpression();

            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());

            var binaryExpression = (BinaryExpressionSyntax)expression;
            Assert.That(binaryExpression.Left, Is.InstanceOf<LiteralExpressionSyntax>());
            Assert.That(binaryExpression.Right, Is.InstanceOf<LiteralExpressionSyntax>());

            var left = (LiteralExpressionSyntax)binaryExpression.Left;
            var right = (LiteralExpressionSyntax)binaryExpression.Right;

            Assert.That(left.LiteralToken, Is.EqualTo(tokens[0]));
            Assert.That(binaryExpression.OperatorToken, Is.EqualTo(tokens[1]));
            Assert.That(right.LiteralToken, Is.EqualTo(tokens[2]));
        }


        public SyntaxToken Number(int value)
            => new SyntaxToken(SyntaxKind.NumberToken, 0, value.ToString(), value);

    }
}