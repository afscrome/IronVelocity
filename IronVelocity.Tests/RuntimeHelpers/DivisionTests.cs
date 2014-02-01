using IronVelocity.Runtime;
using NUnit.Framework;

namespace IronVelocity.Tests.Runtime
{
    public class DivisionTests
    {
        [TestCase(10, 2, 5, TestName = "Division Positive Integer")]
        [TestCase(-10, -2, 5, TestName = "Division Negative Integer")]
        [TestCase(15, -3, -5, TestName = "Division Mixed Integers")]
        [TestCase(null, 5, null, TestName = "Division Null Left")]
        [TestCase(2, null, null, TestName = "Division Null Right")]
        [TestCase(null, null, null, TestName = "Division Null Both")]
        [TestCase(3f, 4, 0.75f, TestName = "Division Integer Float")]
        [TestCase(3, 1.2f, 2.5f, TestName = "Division Float Integer")]
        [TestCase(5, 0, null, TestName = "Division By Integer 0")]
        [TestCase(1.5f, 0f, float.PositiveInfinity, TestName = "Division float by positive 0")]
        [TestCase(1.5f, -0f, float.NegativeInfinity, TestName = "Division float by negative 0")]
        public void BasicTest(object left, object right, object expected)
        {
            var result = Operators.Division(left, right);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DivisionOperatorOverload()
        {
            var left = new OverloadedDivision(6);
            var right = new OverloadedDivision(3);
            var result = Operators.Division(left, right);

            Assert.IsInstanceOf<OverloadedDivision>(result);
            Assert.AreEqual(2, ((OverloadedDivision)result).Value);
        }


        public class OverloadedDivision
        {
            public int Value { get; private set; }
            public OverloadedDivision(int value)
            {
                Value = value;
            }

            public static OverloadedDivision operator /(OverloadedDivision left, OverloadedDivision right)
            {
                return new OverloadedDivision(left.Value / right.Value);
            }
        }
    }
}
