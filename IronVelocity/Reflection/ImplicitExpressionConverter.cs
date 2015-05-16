using IronVelocity.Compilation;
using System;
using System.Linq.Expressions;


namespace IronVelocity.Reflection
{
    public class ImplicitExpressionConverter : IExpressionConverter
    {
        public Expression Convert(Expression target, Type to)
        {
            return VelocityExpressions.ConvertIfNeeded(target, to);
        }
    }
}
