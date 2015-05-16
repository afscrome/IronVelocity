using System;
using System.Linq.Expressions;

namespace IronVelocity.Reflection
{
    public interface IExpressionConverter
    {
        Expression Convert(Expression target, Type to);
    }
}
