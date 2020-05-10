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

	public class ModuloTests : BinaryOperationBinderTestBase
	{

		public override VelocityOperator Operation => VelocityOperator.Modulo;

		[TestCase(5, 3, 2)]
		[TestCase(-5, -3, -2)]
		[TestCase(5, -3, 2)]
		[TestCase(null, 5, null)]
		[TestCase(2, null, null)]
		[TestCase(null, null, null)]
		[TestCase(10, 1.5f, 1f)]
		[TestCase(2, 1.75d, 0.25d)]
		[TestCase(4.5f, 3, 1.5f)]
		[TestCase(285.5f, 140f, 5.5f)]
		[TestCase(1.5525f, 0.45d, 0.20250000953674313d)]
		public void ModuloBasicNumeric(object left, object right, object expectedValue)
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
		[TestCase((float)9, (float)+0f, float.NaN)]
		[TestCase((float)1, (float)-0f, float.NaN)]
		[TestCase((double)2, (double)+0d, double.NaN)]
		[TestCase((double)3, (double)-0d, double.NaN)]
		public void ModuloByZeroTest(object left, object right, object expectedValue)
		{
			MathTest(left, right,expectedValue);
		}

		[TestCase(int.MinValue, -1, null)]
		[TestCase(long.MinValue, -1, null)]
		public void ModuloOverflowTests(object left, object right, object expectedValue)
		{
			MathTest(left, right, expectedValue);
		}

		[Test]
		public void ModuloDecimalDivideByZeroTest()
		{
			MathTest(1m, 0, null);
		}

		[Test]
		public void ModuloUserDefinedOperators()
		{
			var left = new OverloadedMaths(5);
			var right = new OverloadedMaths(3);
			var expected = new OverloadedMaths(2);

			MathTest(left, right, expected);
		}

		[Test]
		public void ModuloEnumNotSupported()
		{
			MathTest(Test.One, Test.Three, null);
		}

	}
}
