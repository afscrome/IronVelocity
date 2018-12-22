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

	public class DivisionTests : BinaryOperationBinderTestBase
	{

		public override VelocityOperator Operation => VelocityOperator.Divide;

		[TestCase(5, 3, 1)]
		[TestCase(-5, -3, 1)]
		[TestCase(5, -3, -1)]
		[TestCase(null, 5, null)]
		[TestCase(2, null, null)]
		[TestCase(null, null, null)]
		[TestCase(1.5f, 4, 0.375f)]
		[TestCase(2.5d, 4, 0.625d)]
		[TestCase(5, 2.5f, 2f)]
		[TestCase(7f, -2.8f, -2.5f)]
		[TestCase(1.5525f, 0.45d, 3.4500000211927624)]
		[TestCase(0, 45, 0)]
		public void DivisionBasicNumeric(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[TestCase((sbyte)1, (sbyte)0, null)]
		[TestCase((short)2, (short)0, null)]
		[TestCase((int)3, (int)0, null)]
		[TestCase((long)4, (long)0, null)]
		[TestCase((byte)5, (byte)0, null)]
		[TestCase((ushort)6, (ushort)0, null)]
		[TestCase((uint)7, (uint)0, null)]
		[TestCase((ulong)8, (ulong)0, null)]
		[TestCase((float)9, (float)+0f, float.PositiveInfinity)]
		[TestCase((float)1, (float)-0f, float.NegativeInfinity)]
		[TestCase((double)2, (double)+0d, double.PositiveInfinity)]
		[TestCase((double)3, (double)-0d, double.NegativeInfinity)]
		public void DivisionByZeroTest(object left, object right, object expectedValue)
		{
			MathTest(left, right,expectedValue);
		}

		[TestCase(int.MinValue, -1, null)]
		[TestCase(long.MinValue, -1, null)]
		public void DivisionOverflowTests(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[Test]
		public void DivisionDecimalDivideByZeroTest()
		{
			MathTest(1m, 0, null);
		}

		[Test]
		public void DivisionDecimalResultTooSmallTest()
		{
			MathTest(1, decimal.MaxValue, 0);
		}

		[Test]
		public void DivisionDecimalOverflowTest()
		{
			MathTest(decimal.MaxValue, 0.5m, null);
		}

		[Test]
		public void DivisionUserDefinedOperators()
		{
			var left = new OverloadedMaths(12);
			var right = new OverloadedMaths(3);
			var expected = new OverloadedMaths(4);

			MathTest(left, right, expected);
		}

		[Test]
		public void DivisionEnumNotSupported()
		{
			MathTest(Test.One, Test.Three, null);
		}

	}
}
