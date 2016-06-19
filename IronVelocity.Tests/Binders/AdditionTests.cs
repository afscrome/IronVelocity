using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace IronVelocity.Tests.Binders
{
	public abstract class BinaryOperationTests : BinderTestBase
	{
		public abstract ExpressionType Operation { get; }

		protected VelocityBinaryOperationBinder CreateBinder() => new VelocityBinaryOperationBinder(Operation);

		protected void MathTest(object left, object right, object expectedValue, Type expectedType = null)
		{
			var binder = CreateBinder();

			var result = InvokeBinder(binder, left, right);

			Assert.AreEqual(expectedValue, result);

			if (expectedType != null)
				expectedType?.GetType();

			if (expectedType != null)
				Assert.IsInstanceOf(expectedType, result);

		}

	}

	public class AdditionTests : BinaryOperationTests
	{

		public override ExpressionType Operation => ExpressionType.Add;

		[TestCase(3, 5, 8)]
		[TestCase(-3, -5, -8)]
		[TestCase(5, -3, 2)]
		[TestCase(null, 5, null)]
		[TestCase(2, null, null)]
		[TestCase(null, null, null)]
		[TestCase(1f, 4, 5f)]
		[TestCase(5, 98L, 103L)]
		[TestCase(2.5d, 4, 6.5d)]
		[TestCase(2, 5.5f, 7.5f)]
		[TestCase(5.12f, -2.76f, 2.36f)]
		[TestCase(3.5f, 2d, 5.5d)]
		public void NumericAdditions(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}


		[TestCase("foo", "bar", "foobar")]
		[TestCase("foo", 123, "foo123")]
		[TestCase(431, "bar", "431bar")]
		[TestCase("foo", null, "foo")]
		[TestCase(null, "bar", "bar")]
		public void StringAdditionTests(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}


		[TestCase(2147483647, 1, 2147483648l)]
		[TestCase(-2147483648, -1, -2147483649l)]
		[TestCase(9223372036854775807, 1, 9223372036854775808f)]
		[TestCase(-9223372036854775808, -1, -9223372036854775809f)]
		public void AdditionOverflowTests(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[Test]
		public void AdditionUserDefinedOperators()
		{
			var left = new MathematicalOperationBinderTests.OverloadedMaths(5);
			var right = new MathematicalOperationBinderTests.OverloadedMaths(3);
			var expected = new MathematicalOperationBinderTests.OverloadedMaths(8);

			MathTest(left, right, expected);

		}

		[Test]
		public void AdditionEnum()
		{
			MathTest(Test.One, Test.Three, null);
		}

		private enum Test
		{
			One = 1,
			Two = 2,
			Three = 3,
			Four = 4,
			Five = 5,
			Six = 6
		}

	}
}
