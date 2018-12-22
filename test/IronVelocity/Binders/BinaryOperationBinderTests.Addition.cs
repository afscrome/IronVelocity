using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using IronVelocity.Reflection;

namespace IronVelocity.Tests.Binders
{

	public class AdditionTests : BinaryOperationBinderTestBase
	{

		public override VelocityOperator Operation => VelocityOperator.Add;

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
		public void AdditionBasicNumeric(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}


		[TestCase("foo", "bar", "foobar")]
		[TestCase("foo", 123, "foo123")]
		[TestCase(431, "bar", "431bar")]
		[TestCase("foo", null, "foo")]
		[TestCase(null, "bar", "bar")]
		public void AdditionStringTests(object left, object right, object expectedValue)
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
			var left = new OverloadedMaths(5);
			var right = new OverloadedMaths(3);
			var expected = new OverloadedMaths(8);

			MathTest(left, right, expected);
		}

		[Test]
		public void AdditionEnumNotSupported()
		{
			MathTest(Test.One, Test.Three, null);
		}

	}
}
