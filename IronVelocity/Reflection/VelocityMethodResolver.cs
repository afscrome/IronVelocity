using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class MethodResolver : IMethodResolver
    {
        private readonly IOverloadResolver _overloadResolver;
        private readonly IArgumentConverter _argumentConverter;

        public MethodResolver(IOverloadResolver overloadResolver, IArgumentConverter argumentConverter)
        {
            _overloadResolver = overloadResolver;
            _argumentConverter = argumentConverter;
        }

        public Expression ConvertMethodParameters(MethodInfo method, Expression target, DynamicMetaObject[] args)//, Type[] argTypeArray)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var argExpressions = _overloadResolver.CreateParameterExpressions(method.GetParameters(), args);

            Expression result = Expression.Call(
                VelocityExpressions.ConvertIfNeeded(target, method.DeclaringType),
                method,
                argExpressions
            );

            if (method.ReturnType == typeof(void))
            {
                result = Expression.Block(
                    result,
                    Constants.VoidReturnValue
                );
            }

            return result;
        }




        public MethodInfo ResolveMethod(TypeInfo type, string name, params Type[] argTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Loosely based on C# resolution algorithm
            // C# 1.0 resolution algorithm at http://msdn.microsoft.com/en-us/library/aa691336(v=vs.71).aspx
            // C# 5.0 algorithm in section 7.5.3 of spec - http://www.microsoft.com/en-gb/download/details.aspx?id=7029

            var candidates = GetCandidateMethods(type, name)
                .Select(x => new FunctionMemberData<MethodInfo>(x, x.GetParameters()));

            return _overloadResolver.Resolve(candidates, argTypes)?.FunctionMember;
        }

        public IEnumerable<MethodInfo> GetCandidateMethods(TypeInfo type, string name)
        {
            return type.GetRuntimeMethods()
                .Where(x => x.IsPublic && !x.IsStatic && !x.IsGenericMethod)
                .Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

    }

}
