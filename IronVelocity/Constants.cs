using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace IronVelocity
{
    public static class Constants
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression InputParameter = Expression.Parameter(typeof(VelocityContext), "_context");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression AsyncStateParameter = Expression.Parameter(typeof(int).MakeByRefType(), "_state");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression AsyncTaskMethodBuilderParameter = Expression.Parameter(typeof(AsyncTaskMethodBuilder), "_state");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression NullExpression = Expression.Constant(null);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression VelocityUnresolvableResult = NullExpression;
    }

}
