using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{

    public interface IMethodResolver
    {
        bool IsArgumentCompatible(Type runtimeType, ParameterInfo parameter);
        MethodInfo ResolveMethod(Type type, string name, params Type[] argTypes);

        Expression ConvertMethodParameters(MethodInfo method, Expression target, DynamicMetaObject[] args);
    }
}
