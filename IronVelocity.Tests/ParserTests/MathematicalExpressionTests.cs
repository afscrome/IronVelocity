using IronVelocity.Parser.AST;
using NUnit.Framework;
using System;
using System.Linq;

namespace IronVelocity.Tests.ParserTests
{
    [TestFixture]
    public class MathematicalExpressionTests
    {

        private class ScrewedUp
        {
            int i = 1;
            int j = 4;

            public int a() { return i++; }
            public int b() { return j++; }
            public int c() { throw new Exception("stuff"); }
        }

        [Test]
        public void Stuff()
        {
            var stuff = new ScrewedUp();
            try
            {
                var x = 1 * 2 * 3 % 4 - 5 / 6;
            }
            catch
            {
                //do nothing
            }
        }

        [TestCase("1 + 2", BinaryOperation.Adddition)]
        [TestCase("1 - 2", BinaryOperation.Subtraction)]
        [TestCase("1 * 2", BinaryOperation.Multiplication)]
        [TestCase("1 / 2", BinaryOperation.Division)]
        [TestCase("1 % 2", BinaryOperation.Modulo)]
        public void TwoOperands(string input, BinaryOperation expectedOperation)
        {
            var parser = new VelocityParserWithStatistics(input);

            var result = parser.CompoundExpression();

            Assert.That(parser.IntegerCallCount, Is.EqualTo(2));
            Assert.That(parser.HasReachedEndOfFile, Is.True);


            Assert.That(result, Is.TypeOf<BinaryExpressionNode>());
            var binaryExpression = (BinaryExpressionNode)result;

            Assert.That(binaryExpression.Operation == expectedOperation);
        }

        [TestCase("1 + 2 + 3", BinaryOperation.Adddition)]
        [TestCase("1 - 2 - 3", BinaryOperation.Subtraction)]
        [TestCase("1 * 2 * 3", BinaryOperation.Multiplication)]
        [TestCase("1 / 2 / 3", BinaryOperation.Division)]
        [TestCase("1 % 2 % 3", BinaryOperation.Modulo)]
        public void ThreeOperandsOfSameOperation(string input, BinaryOperation expectedOperation)
        {
            var parser = new VelocityParserWithStatistics(input);

            var result = parser.CompoundExpression();

            Assert.That(parser.IntegerCallCount, Is.EqualTo(3));
            Assert.That(parser.HasReachedEndOfFile, Is.True);


            Assert.That(result, Is.TypeOf<BinaryExpressionNode>());
            var outerExpression = (BinaryExpressionNode)result;

            Assert.That(outerExpression.Operation == expectedOperation);


            Assert.That(outerExpression.Right, Is.TypeOf<IntegerLiteralNode>());
            Assert.That(outerExpression.Left, Is.TypeOf<BinaryExpressionNode>());
            var innerLeftExpression = (BinaryExpressionNode)outerExpression.Left;
            Assert.That(innerLeftExpression.Operation == expectedOperation);
        }

     


        [TestCase("1 + 2 * 3")]
        [TestCase("1 * 2 + 3")]
        public void MultiplicationHasHigherPrecedenceThanAddition(string input)
        {
            var parser = new VelocityParserWithStatistics(input);
            var result = parser.CompoundExpression();

            Assert.That(parser.IntegerCallCount, Is.EqualTo(3));
            Assert.That(parser.HasReachedEndOfFile, Is.True);


            Assert.That(result, Is.TypeOf<BinaryExpressionNode>());
            var outerExpression = (BinaryExpressionNode)result;

            Assert.That(outerExpression.Operation == BinaryOperation.Adddition);

            var innerBinaryExpression = new[] { outerExpression.Left, outerExpression.Right }.OfType<BinaryExpressionNode>().Single();
            Assert.That(innerBinaryExpression.Operation == BinaryOperation.Multiplication);
        }
    }
}
