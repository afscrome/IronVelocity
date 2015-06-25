using IronVelocity.Binders;
using NUnit.Framework;
using System.Linq.Expressions;

namespace IronVelocity.Tests.Runtime
{
    public class ModuloTests
    {
        [TestCase(10, 3, 1, TestName = "Modulo Positive Integer")]
        [TestCase(-10, -3, -1, TestName = "Modulo Negative Integer")]
        [TestCase(14, -6, 2, TestName = "Modulo Mixed Integers")]
        [TestCase(null, 5, null, TestName = "Modulo Null Left")]
        [TestCase(2, null, null, TestName = "Modulo Null Right")]
        [TestCase(null, null, null, TestName = "Modulo Null Both")]
        [TestCase(6f, 4, 2f, TestName = "Modulo Integer Float")]
        [TestCase(2, 1.5f, 0.5f, TestName = "Modulo Float Integer")]
        [TestCase(5, 0, null, TestName = "Modulo By Integer 0")]
        [TestCase(1.5f, 0f, float.NaN, TestName = "Modulo float by positive 0")]
        [TestCase(1.5f, -0f, float.NaN, TestName = "Modulo float by negative 0")]
        public void BasicTest(object left, object right, object expected)
        {
            var result = Test(left, right);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ModuloOperatorOverloadNotSupported()
        {
            var left = new OverloadedModulo(6);
            var right = new OverloadedModulo(5);
            var result = Test(left, right);

            Assert.Null(result);
        }

        private object Test(object left, object right)
        {
            var binder = new VelocityMathematicalOperationBinder(ExpressionType.Modulo);

            return Utility.BinderTests(binder, left, right);
        }

        public class OverloadedModulo
        {
            public int Value { get; private set; }
            public OverloadedModulo(int value)
            {
                Value = value;
            }

            public static OverloadedModulo operator %(OverloadedModulo left, OverloadedModulo right)
            {
                return new OverloadedModulo(left.Value % right.Value);
            }
        }
    }
}
