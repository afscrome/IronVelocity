using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace IronVelocity.Compilation
{
    public static class MethodHelpers
    {
		internal const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
		internal const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

		public static readonly MethodInfo OutputObjectMethodInfo = typeof(VelocityOutput).GetMethod(nameof(VelocityOutput.Write), new[] { typeof(object) });
        public static readonly MethodInfo OutputStringMethodInfo = typeof(VelocityOutput).GetMethod(nameof(VelocityOutput.Write), new[] { typeof(string) });
        public static readonly MethodInfo OutputStringWithNullFallbackMethodInfo = typeof(VelocityOutput).GetMethod(nameof(VelocityOutput.Write), new[] { typeof(string), typeof(string) });
        public static readonly MethodInfo OutputObjectWithNullFallbackMethodInfo = typeof(VelocityOutput).GetMethod(nameof(VelocityOutput.Write), new[] { typeof(object), typeof(string) });
        public static readonly MethodInfo OutputValueTypeMethodInfo = typeof(VelocityOutput).GetMethod(nameof(VelocityOutput.WriteValueType));

        public static readonly MethodInfo ToStringMethodInfo = typeof(object).GetMethod(nameof(object.ToString), Type.EmptyTypes);

        public static readonly ConstructorInfo ListConstructorInfo = typeof(List<object>).GetConstructor(new[] { typeof(IEnumerable<object>) });
        public static readonly MethodInfo IntegerRangeMethodInfo = typeof(IntegerRange).GetMethod(nameof(IntegerRange.Range), new[] { typeof(int), typeof(int) });
        public static readonly MethodInfo StringConcatMethodInfo = typeof(string).GetMethod(nameof(String.Concat), PublicStatic, null, new[] { typeof(object), typeof(object) }, null);
        public static readonly MethodInfo ReduceBigIntegerMethodInfo = typeof(BigIntegerSimplifier).GetMethod(nameof(BigIntegerSimplifier.ReduceBigInteger), PublicStatic, null, new[] { typeof(BigInteger), typeof(bool) }, null);

        public static readonly MethodInfo BooleanCoercionMethodInfo = typeof(BooleanCoercion).GetMethod(nameof(BooleanCoercion.CoerceToBoolean), PublicStatic, null, new[] { typeof(object) }, null);

		public static readonly MethodInfo ObjectEquals = typeof(object).GetMethod(nameof(object.Equals), PublicStatic, null, new[] { typeof(object), typeof(object) }, null);
		public static readonly MethodInfo GenericStringEnumComparer = typeof(StringEnumComparer).GetMethod(nameof(StringEnumComparer.AreEqual), PublicStatic);
	}
}
