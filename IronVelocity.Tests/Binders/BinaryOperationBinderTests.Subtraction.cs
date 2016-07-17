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
	public class SubtractionTests : BinaryOperationBinderTestBase
	{
		public override VelocityOperator Operation => VelocityOperator.Subtract;

		[TestCase(5, 3, 2)]
		[TestCase(-5, -3, -2)]
		[TestCase(5, -3, 8)]
		[TestCase(null, 5, null)]
		[TestCase(2, null, null)]
		[TestCase(null, null, null)]
		[TestCase(1f, 4, -3f)]
		[TestCase(2.5d, 4, -1.5d)]
		[TestCase(2, 5f, -3f)]
		[TestCase(7.65f, 3.45f, 4.2f)]
		[TestCase(3.5f, 2d, 1.5d)]
		public void SubtractionBasicNumeric(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[TestCase(2147483647, -1, 2147483648)]
		[TestCase(-2147483648, 1, -2147483649)]
		[TestCase(9223372036854775807, -1, 9223372036854775808f)]
		[TestCase(-9223372036854775808, 1, -9223372036854775809f)]
		public void SubtractionOverflowTest(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[Test]
		public void SubtractionUserDefinedOperators()
		{
			var left = new OverloadedMaths(5);
			var right = new OverloadedMaths(3);
			var expected = new OverloadedMaths(2);

			MathTest(left, right, expected);
		}

		[Test]
		public void SubtractionEnumNotSupported()
		{
			MathTest(Test.One, Test.Three, null);
		}


	}
}
