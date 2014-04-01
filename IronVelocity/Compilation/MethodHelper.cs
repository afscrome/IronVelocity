using IronVelocity.Binders;
using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace IronVelocity.Compilation
{
    public static class MethodHelpers
    {
        public static readonly MethodInfo AppendMethodInfo = typeof(StringBuilder).GetMethod("Append", new[] { typeof(string) });
        public static readonly MethodInfo ToStringMethodInfo = typeof(object).GetMethod("ToString", new Type[] { });
        public static readonly ConstructorInfo ListConstructorInfo = typeof(List<object>).GetConstructor(new[] { typeof(IEnumerable<object>) });
        public static readonly MethodInfo IntegerRangeMethodInfo = typeof(IntegerRange).GetMethod("Range", new[] { typeof(int), typeof(int) });
        public static readonly MethodInfo StringConcatMethodInfo = typeof(String).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object), typeof(object) }, null);
        public static readonly MethodInfo ReduceBigIntegerMethodInfo = typeof(VelocityBinaryMathematicalOperationBinder).GetMethod("ReduceBigInteger", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(BigInteger) }, null);

        public static readonly MethodInfo BooleanCoercionMethodInfo = typeof(BooleanCoercion).GetMethod("CoerceToBoolean", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);

        public static readonly MethodInfo DivisionMethodInfo = typeof(Operators).GetMethod("Division", new[] { typeof(object), typeof(object) });
        public static readonly MethodInfo ModuloMethodInfo = typeof(Operators).GetMethod("Modulo", new[] { typeof(object), typeof(object) });

        public static readonly MethodInfo LessThanMethodInfo = typeof(Comparators).GetMethod("LessThan", new[] { typeof(object), typeof(object) });
        public static readonly MethodInfo LessThanOrEqualMethodInfo = typeof(Comparators).GetMethod("LessThanOrEqual", new[] { typeof(object), typeof(object) });
        public static readonly MethodInfo GreaterThanMethodInfo = typeof(Comparators).GetMethod("GreaterThan", new[] { typeof(object), typeof(object) });
        public static readonly MethodInfo GreaterThanOrEqualMethodInfo = typeof(Comparators).GetMethod("GreaterThanOrEqual", new[] { typeof(object), typeof(object) });
        public static readonly MethodInfo EqualMethodInfo = typeof(Comparators).GetMethod("Equal", new[] { typeof(object), typeof(object) });
        public static readonly MethodInfo NotEqualMethodInfo = typeof(Comparators).GetMethod("NotEqual", new[] { typeof(object), typeof(object) });
    }
}
