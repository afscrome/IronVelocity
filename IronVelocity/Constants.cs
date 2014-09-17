using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity
{
    public static class Constants
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression InputParameter = Expression.Parameter(typeof(VelocityContext), "$context");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression AsyncStateParameter = Expression.Parameter(typeof(int).MakeByRefType(), "$state");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression AsyncTaskMethodBuilderParameter = Expression.Parameter(typeof(AsyncTaskMethodBuilder), "_state");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression NullExpression = Expression.Constant(null);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression VelocityUnresolvableResult = NullExpression;

        public static readonly Expression Zero = Expression.Constant(0);

        public static readonly Expression True = Expression.Constant(true);
        public static readonly Expression False = Expression.Constant(false);

        public static readonly Expression EmptyExpression = Expression.Default(typeof(void));
    }

}
