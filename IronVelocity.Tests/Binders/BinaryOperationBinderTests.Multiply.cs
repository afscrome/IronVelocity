using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Binders
{
	public class MultiplicationTests : BinaryOperationBinderTestBase
	{
		public override VelocityOperator Operation => VelocityOperator.Multiply;

		[TestCase(5, 3, 15)]
		[TestCase(-5, -3, 15)]
		[TestCase(5, -3, -15)]
		[TestCase(null, 5, null)]
		[TestCase(2, null, null)]
		[TestCase(null, null, null)]
		[TestCase(1.5f, 4, 6f)]
		[TestCase(2.5d, 4, 10d)]
		[TestCase(2, 4.5f, 9f)]
		[TestCase(5.12f, -2.76f, -14.1312f)]
		[TestCase(3.5f, 2d, 7d)]
		public void MultiplicationBasicNumeric(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[TestCase(2147483647, 2, 4294967294)]
		[TestCase(-2147483648, 3, -6442450944)]
		[TestCase(9223372036854775807, -2, -18446744073709551614d)]
		[TestCase(-9223372036854775808, -2, 18446744073709551616d)]
		public void MultiplicationOverflowTest(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[Test]
		public void MultiplicationUserDefinedOperators()
		{
			var left = new OverloadedMaths(5);
			var right = new OverloadedMaths(3);
			var expected = new OverloadedMaths(15);

			MathTest(left, right, expected);
		}

		[Test]
		public void MultiplicationEnumNotSupported()
		{
			MathTest(Test.One, Test.Three, null);
		}


	}
}
