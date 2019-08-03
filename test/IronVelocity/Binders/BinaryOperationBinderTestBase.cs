using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System;


namespace IronVelocity.Tests.Binders
{
	public abstract class BinaryOperationBinderTestBase : BinderTestBase
	{
		public abstract VelocityOperator Operation { get; }

		protected VelocityBinaryOperationBinder CreateBinder() => new VelocityBinaryOperationBinder(Operation, new OperatorResolver(new OverloadResolver(new ArgumentConverter())));

		protected void MathTest(object left, object right, object expectedValue, Type expectedType = null)
		{
			var binder = CreateBinder();

			var result = InvokeBinder(binder, left, right);

			Assert.AreEqual(expectedValue, result);

			if (expectedType != null)
				expectedType?.GetType();

			if (expectedType != null)
				Assert.AreEqual(expectedType, result.GetType());

		}

		public class OverloadedMaths : IEquatable<OverloadedMaths>
		{
			public int Value { get; }
			public OverloadedMaths(int value)
			{
				Value = value;
			}

			public static OverloadedMaths operator +(OverloadedMaths left, OverloadedMaths right)
				=> new OverloadedMaths(left.Value + right.Value);

			public static OverloadedMaths operator -(OverloadedMaths left, OverloadedMaths right)
				=> new OverloadedMaths(left.Value - right.Value);

			public static OverloadedMaths operator *(OverloadedMaths left, OverloadedMaths right)
				=> new OverloadedMaths(left.Value * right.Value);

			public static OverloadedMaths operator /(OverloadedMaths left, OverloadedMaths right)
				=> new OverloadedMaths(left.Value / right.Value);

			public static OverloadedMaths operator %(OverloadedMaths left, OverloadedMaths right)
				=> new OverloadedMaths(left.Value % right.Value);


			public override string ToString()
				=> "OverloadedMath " + Value;

            public bool Equals(OverloadedMaths other)
            {
                return Value == other.Value;
            }
        }

		public enum Test
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
