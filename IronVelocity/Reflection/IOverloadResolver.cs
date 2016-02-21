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
        OverloadResolutionData<T> Resolve<T>(IEnumerable<FunctionMemberData<T>> candidates, IImmutableList<Type> args)
            where T : MemberInfo;
        IImmutableList<Expression> CreateParameterExpressions<T>(OverloadResolutionData<T> overload, DynamicMetaObject[] args)
            where T : MemberInfo;
    }
}
