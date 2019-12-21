using IronVelocity.Compilation;
using System;
using System.Linq.Expressions;


namespace IronVelocity.Reflection
{
    public class ImplicitExpressionConverter : IExpressionConverter
    {
        public Expression Convert(Expression target, Type to) => VelocityExpressions.ConvertIfNeeded(target, to);
    }
}
