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
    }
}
