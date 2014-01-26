using System;
using System.Linq.Expressions;
using System.Text;

namespace IronVelocity
{
    public static class Constants
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression OutputParameter = Expression.Parameter(typeof(StringBuilder), "_output");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression InputParameter = Expression.Parameter(typeof(VelocityContext), "_context");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression NullExpression = Expression.Constant(null);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly Expression VelocityUnresolvableResult = NullExpression;
    }

}
