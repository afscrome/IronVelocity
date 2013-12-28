﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IronVelocity
{
    public static class Constants
    {
        public static readonly ParameterExpression OutputParameter = Expression.Parameter(typeof(StringBuilder), "_output");
        public static readonly ParameterExpression InputParameter = Expression.Parameter(typeof(IDictionary<string, object>), "_input");
        public static readonly ParameterExpression EnvironmentParameter = Expression.Parameter(typeof(Environment), "_environment");

        public static readonly Expression NullExpression = Expression.Constant(null);
        public static readonly Expression VelocityUnresolvableResult = NullExpression;
        public static readonly Expression VelocityAmbigiousMatchResult = NullExpression;
    }

}
