using IronVelocity.Binders;
using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace IronVelocity.Compilation
{
    public static class MethodHelpers
    {
        public static readonly MethodInfo AppendMethodInfo = typeof(StringBuilder).GetMethod("Append", new[] { typeof(object) });
        public static readonly MethodInfo AppendStringMethodInfo = typeof(StringBuilder).GetMethod("Append", new[] { typeof(string) });

        public static readonly MethodInfo ToStringMethodInfo = typeof(object).GetMethod("ToString", new Type[] { });

        public static readonly ConstructorInfo ListConstructorInfo = typeof(List<object>).GetConstructor(new[] { typeof(IEnumerable<object>) });
        public static readonly MethodInfo IntegerRangeMethodInfo = typeof(IntegerRange).GetMethod("Range", new[] { typeof(int), typeof(int) });
        public static readonly MethodInfo StringConcatMethodInfo = typeof(String).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object), typeof(object) }, null);
        public static readonly MethodInfo ReduceBigIntegerMethodInfo = typeof(BinaryOperationHelper).GetMethod("ReduceBigInteger", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(BigInteger) }, null);

        public static readonly MethodInfo BooleanCoercionMethodInfo = typeof(BooleanCoercion).GetMethod("CoerceToBoolean", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);

        public static readonly MethodInfo SetAsyncMethodBuilderStateMachine = typeof(AsyncTaskMethodBuilder).GetMethod("SetStateMachine", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(IAsyncStateMachine) }, null);

        public static readonly ConstructorInfo DebuggableAttributeConstructorInfo = typeof(DebuggableAttribute).GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
        public static readonly ConstructorInfo DebuggerHiddenConstructorInfo = typeof(DebuggerHiddenAttribute).GetConstructor((new Type[] { }));
    }
}
