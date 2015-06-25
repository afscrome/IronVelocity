using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Tests.Binders
{
    [TestFixture]
    public class MathematicalOperationBinderTests
    {
        [TestCase(3, 5, 8, typeof(int), TestName = "Addition Positive Integer")]
        [TestCase(-3, -5, -8, typeof(int), TestName = "Addition Negative Integer")]
        [TestCase(5, -3, 2, typeof(int), TestName = "Addition Mixed Integers")]
        [TestCase(null, 5, null, null, TestName = "Addition Null Left")]
        [TestCase(2, null, null, null, TestName = "Addition Null Right")]
        [TestCase(null, null, null, null, TestName = "Addition Null Both")]
        [TestCase(1f, 4, 5f, typeof(float), TestName = "Addition Integer Float")]
        [TestCase(5, 98L, 103L, typeof(long), TestName = "Addition Integer Long")]
        [TestCase(2.5d, 4, 6.5d, typeof(double), TestName = "Addition Integer Double")]
        [TestCase(2, 5.5f, 7.5f, typeof(float), TestName = "Addition Float Integer")]
        [TestCase(5.12f, -2.76f, 2.36f, typeof(float), TestName = "Addition Float Float")]
        [TestCase(3.5f, 2d, 5.5d, typeof(double), TestName = "Addition Float Double")]
        [TestCase(2147483647, 1, 2147483648, typeof(long), TestName = "Addition Integer Overflow")]
        [TestCase(-2147483648, -1, -2147483649, typeof(long), TestName = "Addition Integer Underflow")]
        [TestCase(9223372036854775807, 1, 9223372036854775808f, typeof(float), TestName = "Addition Long Overflow")]
        [TestCase(-9223372036854775808, -1, -9223372036854775809f, typeof(float), TestName = "Addition Long Underflow")]
        public void BasicAdditionTests(object left, object right, object expectedValue, Type expectedType)
        {
            MathTest(left, right, ExpressionType.Add, expectedValue, expectedType);
        }



        [TestCase(5, 3, 2, typeof(int), TestName = "Subtraction Positive Integer")]
        [TestCase(-5, -3, -2, typeof(int), TestName = "Subtraction Negative Integer")]
        [TestCase(5, -3, 8, typeof(int), TestName = "Subtraction Mixed Integers")]
        [TestCase(null, 5, null, null,TestName = "Subtraction Null Left")]
        [TestCase(2, null, null, null, TestName = "Subtraction Null Right")]
        [TestCase(null, null, null, null, TestName = "Subtraction Null Both")]
        [TestCase(1f, 4, -3f, typeof(float), TestName = "Subtraction Integer Float")]
        [TestCase(2.5d, 4, -1.5d, typeof(double), TestName = "Subtraction Integer Double")]
        [TestCase(2, 5f, -3f, typeof(float), TestName = "Subtraction Float Integer")]
        [TestCase(7.65f, 3.45f, 4.2f, typeof(float), TestName = "Subtraction Float Float")]
        [TestCase(3.5f, 2d, 1.5d, typeof(double), TestName = "Addition Float Double")]
        [TestCase(2147483647, -1, 2147483648, typeof(long), TestName = "Subtraction Integer Overflow")]
        [TestCase(-2147483648, 1, -2147483649, typeof(long), TestName = "Subtraction Integer Underflow")]
        [TestCase(9223372036854775807, -1, 9223372036854775808f, typeof(float), TestName = "Subtraction Long Overflow")]
        [TestCase(-9223372036854775808, 1, -9223372036854775809f, typeof(float), TestName = "Subtraction Long Underflow")]
        public void SubtractionTest(object left, object right, object expectedValue, Type expectedType)
        {
            MathTest(left, right, ExpressionType.Subtract, expectedValue, expectedType);
        }


        [TestCase(5, 3, 15, typeof(int), TestName = "Multiplication Positive Integer")]
        [TestCase(-5, -3, 15, typeof(int), TestName = "Multiplication Negative Integer")]
        [TestCase(5, -3, -15, typeof(int), TestName = "Multiplication Mixed Integers")]
        [TestCase(null, 5, null, null, TestName = "Multiplication Null Left")]
        [TestCase(2, null, null, null, TestName = "Multiplication Null Right")]
        [TestCase(null, null, null, null, TestName = "Multiplication Null Both")]
        [TestCase(1.5f, 4, 6f, typeof(float), TestName = "Multiplication Integer Float")]
        [TestCase(2.5d, 4, 10d, typeof(double), TestName = "Multiplication Integer Double")]
        [TestCase(2, 4.5f, 9f, typeof(float), TestName = "Multiplication Float Integer")]
        [TestCase(5.12f, -2.76f, -14.1312f, typeof(float), TestName = "Multiplication Float Float")]
        [TestCase(3.5f, 2d, 7d, typeof(double), TestName = "Multiplication Float Double")]
        [TestCase(2147483647, 2, 4294967294, typeof(long), TestName = "Multiplication Integer Overflow")]
        [TestCase(-2147483648, 3, -6442450944, typeof(long), TestName = "Multiplication Integer Underflow")]
        [TestCase(9223372036854775807, -2, -18446744073709551614d, typeof(float), TestName = "Multiply Long Overflow")]
        [TestCase(-9223372036854775808, -2, 18446744073709551616d, typeof(float), TestName = "Multiply Long Underflow")]
        public void BasicMultiplicationTest(object left, object right, object expectedValue, Type expectedType)
        {
            MathTest(left, right, ExpressionType.Multiply, expectedValue, expectedType);
        }

        [TestCase(5, 3, 1, typeof(int), TestName = "Division Positive Integer")]
        [TestCase(-5, -3, 1, typeof(int), TestName = "Division Negative Integer")]
        [TestCase(5, -3, -1, typeof(int), TestName = "Division Mixed Integers")]
        [TestCase(null, 5, null, null, TestName = "Division Null Left")]
        [TestCase(2, null, null, null, TestName = "Division Null Right")]
        [TestCase(null, null, null, null, TestName = "Division Null Both")]
        [TestCase(1.5f, 4, 0.375f, typeof(float), TestName = "Division Integer Float")]
        [TestCase(2.5d, 4, 0.625d, typeof(double), TestName = "Division Integer Double")]
        [TestCase(5, 2.5f, 2f, typeof(float), TestName = "Division Float Integer")]
        [TestCase(7f, -2.8f, -2.5f, typeof(float), TestName = "Division Float Float")]
        [TestCase(1.5525f, 0.45d, 3.4500000211927624, typeof(double), TestName = "Division Float Double")]
        [TestCase(1, 0, null, null, TestName = "Division By Zero")]
        public void BasicDivisionTest(object left, object right, object expectedValue, Type expectedType)
        {
            MathTest(left, right, ExpressionType.Divide, expectedValue, expectedType);
        }


        [TestCase(5, 3, 2, typeof(int), TestName = "Modulo Positive Integer")]
        [TestCase(-5, -3, -2, typeof(int), TestName = "Modulo Negative Integer")]
        [TestCase(5, -3, 2, typeof(int), TestName = "Modulo Mixed Integers")]
        [TestCase(null, 5, null, null, TestName = "Modulo Null Left")]
        [TestCase(2, null, null, null, TestName = "Modulo Null Right")]
        [TestCase(null, null, null, null, TestName = "Modulo Null Both")]
        [TestCase(10, 1.5f, 1f, null, TestName = "Modulo Integer Float")]
        [TestCase(2, 1.75d, 0.25d, null, TestName = "Modulo Integer Double")]
        [TestCase(4.5f, 3, 1.5f, null, TestName = "Modulo Float Integer")]
        [TestCase(285.5f, 140f, 5.5f, typeof(float), TestName = "Modulo Float Float")]
        [TestCase(1.5525f, 0.45d, 0.20250000953674313d, typeof(double), TestName = "Modulo Float Double")]
        [TestCase(1, 0, null, null, TestName = "Modulo By Zero")]
        public void BasicModuloTest(object left, object right, object expected, Type expectedType)
        {
            MathTest(left, right, ExpressionType.Modulo, expected, expectedType);
        }

        [TestCase(ExpressionType.Add)]
        [TestCase(ExpressionType.Subtract)]
        [TestCase(ExpressionType.Multiply)]
        [TestCase(ExpressionType.Divide)]
        [TestCase(ExpressionType.Modulo)]
        public void OperatorOverloadNotSupported(ExpressionType type)
        {
            var left = new OverloadedMaths(1);
            var right = new OverloadedMaths(3);
            MathTest(left, right, type, expectedValue: null, expectedType: null);
        }


        private void MathTest(object left, object right, ExpressionType expressionType, object expectedValue, Type expectedType)
        {
            var binder = new VelocityMathematicalOperationBinder(expressionType);

            var result = Utility.BinderTests(binder, left, right);

            if (expectedType != null)
                Assert.IsInstanceOf(expectedType, result);

            Assert.AreEqual(expectedValue, result);
        }

        public class OverloadedMaths
        {
            public int Value { get; private set; }
            public OverloadedMaths(int value)
            {
                Value = value;
            }

            public static OverloadedMaths operator +(OverloadedMaths left, OverloadedMaths right)
            {
                return new OverloadedMaths(left.Value + right.Value);
            }

            public static OverloadedMaths operator -(OverloadedMaths left, OverloadedMaths right)
            {
                return new OverloadedMaths(left.Value - right.Value);
            }

            public static OverloadedMaths operator *(OverloadedMaths left, OverloadedMaths right)
            {
                return new OverloadedMaths(left.Value * right.Value);
            }

            public static OverloadedMaths operator /(OverloadedMaths left, OverloadedMaths right)
            {
                return new OverloadedMaths(left.Value / right.Value);
            }

            public static OverloadedMaths operator %(OverloadedMaths left, OverloadedMaths right)
            {
                return new OverloadedMaths(left.Value % right.Value);
            }
        }
    }
}
