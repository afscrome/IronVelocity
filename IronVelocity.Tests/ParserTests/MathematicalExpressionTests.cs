using IronVelocity.Parser;
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
            var parser = new VelocityParserWithStatistics(input, LexerState.Vtl);

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
            var parser = new VelocityParserWithStatistics(input, LexerState.Vtl);

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




        [TestCase("1 + 2 - 3", BinaryOperation.Subtraction, BinaryOperation.Adddition, true)]
        [TestCase("1 + 2 * 3", BinaryOperation.Adddition, BinaryOperation.Multiplication, false)]
        [TestCase("1 + 2 / 3", BinaryOperation.Adddition, BinaryOperation.Division, false)]
        [TestCase("1 + 2 % 3", BinaryOperation.Adddition, BinaryOperation.Modulo, false)]
        
        [TestCase("1 - 2 + 3", BinaryOperation.Adddition, BinaryOperation.Subtraction, true)]
        [TestCase("1 - 2 * 3", BinaryOperation.Subtraction, BinaryOperation.Multiplication, false)]
        [TestCase("1 - 2 / 3", BinaryOperation.Subtraction, BinaryOperation.Division, false)]
        [TestCase("1 - 2 % 3", BinaryOperation.Subtraction, BinaryOperation.Modulo, false)]

        [TestCase("1 * 2 + 3", BinaryOperation.Adddition, BinaryOperation.Multiplication, true)]
        [TestCase("1 * 2 - 3", BinaryOperation.Subtraction, BinaryOperation.Multiplication, true)]
        [TestCase("1 * 2 / 3", BinaryOperation.Division, BinaryOperation.Multiplication, true)]
        [TestCase("1 * 2 % 3", BinaryOperation.Modulo, BinaryOperation.Multiplication, true)]

        [TestCase("1 / 2 + 3", BinaryOperation.Adddition, BinaryOperation.Division, true)]
        [TestCase("1 / 2 - 3", BinaryOperation.Subtraction, BinaryOperation.Division, true)]
        [TestCase("1 / 2 * 3", BinaryOperation.Multiplication, BinaryOperation.Division, true)]
        [TestCase("1 / 2 % 3", BinaryOperation.Modulo, BinaryOperation.Division, true)]

        [TestCase("1 % 2 + 3", BinaryOperation.Adddition, BinaryOperation.Modulo, true)]
        [TestCase("1 % 2 - 3", BinaryOperation.Subtraction, BinaryOperation.Modulo, true)]
        [TestCase("1 % 2 * 3", BinaryOperation.Multiplication, BinaryOperation.Modulo, true)]
        [TestCase("1 % 2 / 3", BinaryOperation.Division, BinaryOperation.Modulo, true)]
        public void HigherPrecedenceOperationsAreDeeperInTreeThanLowerPrecedence(string input, BinaryOperation outerOperation, BinaryOperation innerOperation, bool innerBinaryExpressionIsOnLeft)
        {
            var parser = new VelocityParserWithStatistics(input, LexerState.Vtl);
            var result = parser.CompoundExpression();

            Assert.That(parser.IntegerCallCount, Is.EqualTo(3));
            Assert.That(parser.HasReachedEndOfFile, Is.True);


            Assert.That(result, Is.TypeOf<BinaryExpressionNode>());
            var outerExpression = (BinaryExpressionNode)result;

            Assert.That(outerExpression.Operation == outerOperation);

            var innerExpression = innerBinaryExpressionIsOnLeft
                ?outerExpression.Left
                : outerExpression.Right;
            Assert.That(innerExpression, Is.TypeOf<BinaryExpressionNode>());

            var innerBinaryExpression = (BinaryExpressionNode)innerExpression;
            Assert.That(innerBinaryExpression.Operation == innerOperation);
        }
    }
}
