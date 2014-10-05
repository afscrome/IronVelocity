using IronVelocity.Compilation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity
{
    public static class Constants
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression InputParameter = Expression.Parameter(typeof(VelocityContext), "$context");

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression AsyncStateParameter = Expression.Parameter(typeof(int).MakeByRefType(), "$state");

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression AsyncTaskMethodBuilderParameter = Expression.Parameter(typeof(AsyncTaskMethodBuilder).MakeByRefType(), "$builder");

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression TaskAwaiterParameter = Expression.Parameter(typeof(TaskAwaiter).MakeByRefType(), "$awaiter");

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression StateMachineParameter = Expression.Parameter(typeof(VelocityAsyncCompiler.VelocityAsyncStateMachine).MakeByRefType(), "$this");

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression NullExpression = Expression.Constant(null);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression VelocityUnresolvableResult = NullExpression;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression Zero = Expression.Constant(0);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression True = Expression.Constant(true);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression False = Expression.Constant(false);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression EmptyExpression = Expression.Default(typeof(void));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression VoidReturnValue = Expression.Constant(String.Empty);
    }

}
