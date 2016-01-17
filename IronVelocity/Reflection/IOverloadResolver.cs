using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public interface IOverloadResolver
    {
        FunctionMemberData<T> Resolve<T>(IEnumerable<FunctionMemberData<T>> candidates, IImmutableList<Type> args);
        IImmutableList<Expression> CreateParameterExpressions(ParameterInfo[] parameters, DynamicMetaObject[] args);
    }
}
