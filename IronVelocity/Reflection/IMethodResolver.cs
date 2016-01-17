using System;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{

    public interface IMethodResolver
    {
        MethodInfo ResolveMethod(TypeInfo type, string name, IImmutableList<Type> argTypes);

        Expression ConvertMethodParameters(MethodInfo method, Expression target, DynamicMetaObject[] args);
    }
}
