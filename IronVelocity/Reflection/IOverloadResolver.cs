using System;
using System.Collections.Generic;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public interface IOverloadResolver
    {
        FunctionMemberData<T> Resolve<T>(IEnumerable<FunctionMemberData<T>> candidates, Type[] args);
        bool IsParameterArrayArgument(ParameterInfo parameter);
    }
}
