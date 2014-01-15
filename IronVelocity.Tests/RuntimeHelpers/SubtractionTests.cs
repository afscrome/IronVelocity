﻿using IronVelocity.RuntimeHelpers;
using NUnit.Framework;

namespace IronVelocity.Tests.RuntimeHelpers
{
    public class SubtractionTests
    {
        [TestCase(5, 3, 2, TestName = "Subtraction Positive Integer")]
        [TestCase(-5, -3, -2, TestName = "Subtraction Negative Integer")]
        [TestCase(5, -3, 8, TestName = "Subtraction Mixed Integers")]
        [TestCase(null, 5, null, TestName = "Subtraction Null Left")]
        [TestCase(2, null, null, TestName = "Subtraction Null Right")]
        [TestCase(null, null, null, TestName = "Subtraction Null Both")]
        [TestCase(1f, 4, -3f, TestName = "Subtraction Integer Float")]
        [TestCase(2, 5f, -3f, TestName = "Subtraction Float Integer")]
        //[TestCase(2147483647, 1, 2147483648, TestName = "Subtraction Integer Overflow")]
        //[TestCase(-2147483648, -1, -2147483649, TestName = "Subtraction Integer Underflow")]
        public void BasicTest(object left, object right, object expected)
        {
            var result = Operators.Subtraction(left, right);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SubtractionOperatorOverload()
        {
            var left = new OverloadedSubtraction(1);
            var right = new OverloadedSubtraction(3);
            var result = Operators.Subtraction(left, right);

            Assert.IsInstanceOf<OverloadedSubtraction>(result);
            Assert.AreEqual(-2, ((OverloadedSubtraction)result).Value);
        }



        public class OverloadedSubtraction
        {
            public int Value { get; private set; }
            public OverloadedSubtraction(int value)
            {
                Value = value;
            }

            public static OverloadedSubtraction operator -(OverloadedSubtraction left, OverloadedSubtraction right)
            {
                return new OverloadedSubtraction(left.Value - right.Value);
            }
        }
    }
}