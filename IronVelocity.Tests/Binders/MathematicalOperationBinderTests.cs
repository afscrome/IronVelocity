using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using OverloadedMaths = IronVelocity.Tests.Binders.BinaryOperationBinderTestBase.OverloadedMaths;

namespace IronVelocity.Tests.Binders
{
    [TestFixture]
    public class MathematicalOperationBinderTests : BinderTestBase
    {
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
            MathTest(left, right, MathematicalOperation.Divide, expectedValue, expectedType);
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
            MathTest(left, right, MathematicalOperation.Modulo, expected, expectedType);
        }


        [TestCase(MathematicalOperation.Divide)]
        [TestCase(MathematicalOperation.Modulo)]
        public void OverloadedMathematicalOperation(MathematicalOperation operation)
        {
            var left = new OverloadedMaths(1);
            var right = new OverloadedMaths(3);
            MathTest(left, right, operation, expectedValue: null, expectedType: null);
        }


        private void MathTest(object left, object right, MathematicalOperation operation, object expectedValue, Type expectedType)
        {
            var binder = new VelocityMathematicalOperationBinder(operation, new ArgumentConverter());

            var result = InvokeBinder(binder, left, right);

            if (expectedType != null)
                Assert.IsInstanceOf(expectedType, result);

            Assert.AreEqual(expectedValue, result);
        }


	}
}
