using IronVelocity.Binders;
using NUnit.Framework;
using System.Linq.Expressions;
using Tests;

namespace IronVelocity.Tests.Runtime
{
    public class AdditionTests
    {
        [TestCase(3, 5, 8, TestName = "Addition Positive Integer")]
        [TestCase(-3, -5, -8, TestName = "Addition Negative Integer")]
        [TestCase(5, -3, 2, TestName = "Addition Mixed Integers")]
        [TestCase(null, 5, null, TestName = "Addition Null Left")]
        [TestCase(2, null, null, TestName = "Addition Null Right")]
        [TestCase(null, null, null, TestName = "Addition Null Both")]
        [TestCase(1f, 4, 5f, TestName = "Addition Integer Float")]
        [TestCase(2.5d, 4, 6.5d, TestName = "Addition Integer Double")]
        [TestCase(2, 5.5f, 7.5f, TestName = "Addition Float Integer")]
        [TestCase(3.5f, 2d, 5.5d, TestName = "Addition Float Double")]
        [TestCase(2147483647, 1, 2147483648, TestName = "Addition Integer Overflow")]
        [TestCase(-2147483648, -1, -2147483649, TestName = "Addition Integer Underflow")]
        public void BasicTest(object left, object right, object expected)
        {
            var result = Test(left, right);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AdditionOperatorOverload()
        {
            Assert.Inconclusive("TODO: Determine support for custom operators");

            var left = new OverloadedAdd(1);
            var right = new OverloadedAdd(3);
            var result = Test(left, right);

            Assert.IsInstanceOf<OverloadedAdd>(result);
            Assert.AreEqual(4, ((OverloadedAdd)result).Value);
        }

        private object Test(object left, object right)
        {
            var binder = new VelocityBinaryMathematicalOperationBinder(ExpressionType.Add);

            return Utility.BinderTests(binder, left, right);
        }

        public class OverloadedAdd
        {
            public int Value { get; private set; }
            public OverloadedAdd(int value)
            {
                Value = value;
            }

            public static OverloadedAdd operator +(OverloadedAdd left, OverloadedAdd right)
            {
                return new OverloadedAdd(left.Value + right.Value);
            }
        }
    }
}
