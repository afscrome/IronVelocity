using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.RuntimeHelpers;

namespace IronVelocity.Tests.RuntimeHelpers
{
    public class MultiplicationTests
    {
        [TestCase(5, 3, 15, TestName = "Multiplication Positive Integer")]
        [TestCase(-5, -3, 15, TestName = "Multiplication Negative Integer")]
        [TestCase(5, -3, -15, TestName = "Multiplication Mixed Integers")]
        [TestCase(null, 5, null, TestName = "Multiplication Null Left")]
        [TestCase(2, null, null, TestName = "Multiplication Null Right")]
        [TestCase(null, null, null, TestName = "Multiplication Null Both")]
        [TestCase(1.5f, 4, 6f, TestName = "Multiplication Integer Float")]
        [TestCase(2, 4.5f, 9f, TestName = "Multiplication Float Integer")]
        //[TestCase(2147483647, 1, 2147483648, TestName = "Multiplication Integer Overflow")]
        //[TestCase(-2147483648, -1, -2147483649, TestName = "Multiplication Integer Underflow")]
        public void BasicTest(object left, object right, object expected)
        {
            var result = Operators.Multiplication(left, right);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MultiplicationOperatorOverload()
        {
            var left = new OverloadedMultiplication(1);
            var right = new OverloadedMultiplication(3);
            var result = Operators.Multiplication(left, right);

            Assert.IsInstanceOf<OverloadedMultiplication>(result);
            Assert.AreEqual(3, ((OverloadedMultiplication)result).Value);
        }


        public class OverloadedMultiplication
        {
            public int Value { get; private set; }
            public OverloadedMultiplication(int value)
            {
                Value = value;
            }

            public static OverloadedMultiplication operator *(OverloadedMultiplication left, OverloadedMultiplication right)
            {
                return new OverloadedMultiplication(left.Value * right.Value);
            }
        }
    }
}
