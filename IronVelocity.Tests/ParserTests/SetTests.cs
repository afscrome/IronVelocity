using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace IronVelocity.Tests.ParserTests
{
    public class SetTests
    {
        //BasicSet
        //Set swallows trailing whitespace
        //Set swallows leading whitespace
        [Test]
        public void BasicSet()
        {
            var input = "#set($x = 123)";
            var mockParser = new Mock<VelocityParser>(input);
            mockParser.CallBase = true;
            var parser = mockParser.Object;

            var result = parser.Parse();

            mockParser.Verify(x => x.SetDirective(), Times.Once);
            Assert.That(parser.HasReachedEndOfFile);

            Assert.That(result.Children, Has.Length.EqualTo(1));
            Assert.That(result.Children.First(), Is.TypeOf<BinaryExpressionNode>());

            var assignment = (BinaryExpressionNode)result.Children.First();
            Assert.That(assignment.Operation, Is.EqualTo(BinaryOperation.Assignment));
            Assert.That(assignment.Left, Is.TypeOf<ReferenceNode>());
            Assert.That(assignment.Right, Is.TypeOf<IntegerLiteralNode>());
        }

        [TestCase("#set($foo = 'ya')\t ")]
        [TestCase("#set($foo = 'ya') \r\n")]
        [TestCase("#set($foo = 'ya')\t\n")]
        [TestCase(" \t #set($foo = 'ya')", Ignore =true, IgnoreReason ="Not yet implemented")]
        [TestCase(" \t #set($foo = 'ya') \r\n", Ignore = true, IgnoreReason = "Not yet implemented")]
        public void SetEatsSurroundingWhitespace(string input)
        {
            var mockParser = new Mock<VelocityParser>(input);
            mockParser.CallBase = true;
            var parser = mockParser.Object;

            var result = parser.Parse();

            mockParser.Verify(x => x.SetDirective(), Times.Once);
            Assert.That(parser.HasReachedEndOfFile);

            Assert.That(result.Children, Has.Length.EqualTo(1));
            Assert.That(result.Children.First(), Is.TypeOf<BinaryExpressionNode>());

            var assignment = (BinaryExpressionNode)result.Children.First();
            Assert.That(assignment.Operation, Is.EqualTo(BinaryOperation.Assignment));
        }


        [TestCase("#set($foo = 'ya')  \r\n  HELLO")]
        [TestCase("#set($foo = 'ya')  \n  HELLO")]
        public void SetDoesNotEatWhitespaceOnNextLine(string input)
        {
            var mockParser = new Mock<VelocityParser>(input);
            mockParser.CallBase = true;
            var parser = mockParser.Object;

            var result = parser.Parse();

            mockParser.Verify(x => x.SetDirective(), Times.Once);
            Assert.That(parser.HasReachedEndOfFile);

            Assert.That(result.Children, Has.Length.EqualTo(2));
            Assert.That(result.Children.Last(), Is.TypeOf<TextNode>());

            var text = (TextNode)result.Children.Last();
            Assert.That(text.Content, Is.EqualTo("  HELLO"));
        }
    }
}
