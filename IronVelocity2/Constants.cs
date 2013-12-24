using System;
using System.Linq.Expressions;
using System.Text;

namespace IronVelocity
{
    public static class Constants
    {
        public static readonly ParameterExpression OutputParameter = Expression.Parameter(typeof(StringBuilder), "_output");
        public static readonly ParameterExpression EnvironmentParameter = Expression.Parameter(typeof(Environment), "_environment");
    }

}
