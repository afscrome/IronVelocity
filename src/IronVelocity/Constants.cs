﻿using IronVelocity.Runtime;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace IronVelocity
{
    public static class Constants
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression InputParameter = Expression.Parameter(typeof(VelocityContext), "context");

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Expressions are immutable")]
        public static readonly ParameterExpression OutputParameter = Expression.Parameter(typeof(VelocityOutput), "output");

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
        public static readonly Expression VoidReturnValue = Expression.Constant(string.Empty);
    }

}
