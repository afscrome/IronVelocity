using IronVelocity.Binders;
using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IronVelocity.Compilation
{
    public static class MethodHelpers
    {
        public static readonly MethodInfo OutputObjectMethodInfo = typeof(VelocityOutput).GetMethod("Write", new[] { typeof(object) });
        public static readonly MethodInfo OutputStringMethodInfo = typeof(VelocityOutput).GetMethod("Write", new[] { typeof(string) });
        public static readonly MethodInfo OutputStringWithNullFallbackMethodInfo = typeof(VelocityOutput).GetMethod("Write", new[] { typeof(string), typeof(string) });
        public static readonly MethodInfo OutputObjectWithNullFallbackMethodInfo = typeof(VelocityOutput).GetMethod("Write", new[] { typeof(object), typeof(string) });
        public static readonly MethodInfo OutputValueTypeMethodInfo = typeof(VelocityOutput).GetMethod("WriteValueType");

        public static readonly MethodInfo ToStringMethodInfo = typeof(object).GetMethod("ToString", new Type[] { });

        public static readonly ConstructorInfo ListConstructorInfo = typeof(List<object>).GetConstructor(new[] { typeof(IEnumerable<object>) });
        public static readonly MethodInfo IntegerRangeMethodInfo = typeof(IntegerRange).GetMethod("Range", new[] { typeof(int), typeof(int) });
        public static readonly MethodInfo StringConcatMethodInfo = typeof(String).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object), typeof(object) }, null);
        public static readonly MethodInfo ReduceBigIntegerMethodInfo = typeof(BinaryOperationHelper).GetMethod("ReduceBigInteger", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(BigInteger), typeof(bool) }, null);

        public static readonly MethodInfo BooleanCoercionMethodInfo = typeof(BooleanCoercion).GetMethod("CoerceToBoolean", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);

        public static readonly MethodInfo SetAsyncMethodBuilderStateMachine = typeof(AsyncTaskMethodBuilder).GetMethod("SetStateMachine", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(IAsyncStateMachine) }, null);

        public static readonly ConstructorInfo DebuggableAttributeConstructorInfo = typeof(DebuggableAttribute).GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
        public static readonly ConstructorInfo DebuggerHiddenConstructorInfo = typeof(DebuggerHiddenAttribute).GetConstructor((new Type[] { }));
    }
}
