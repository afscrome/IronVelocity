using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class MethodResolver : IMethodResolver
    {
        private readonly IOverloadResolver _overloadResolver;

        public MethodResolver(IOverloadResolver overloadResolver)
        {
            _overloadResolver = overloadResolver;
        }

        public Expression ConvertMethodParameters(OverloadResolutionData<MethodInfo> resolvedMethod, Expression target, DynamicMetaObject[] args)//, Type[] argTypeArray)
        {
            if (resolvedMethod == null)
                throw new ArgumentNullException(nameof(resolvedMethod));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var argExpressions = _overloadResolver.CreateParameterExpressions(resolvedMethod, args);
            var method = resolvedMethod.FunctionMember;

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
	

        public OverloadResolutionData<MethodInfo> ResolveMethod(TypeInfo type, string name, IImmutableList<Type> argTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

			IEnumerable<MethodInfo> candidateMethods;

			if (typeof(Delegate).IsAssignableFrom(type))
				candidateMethods = new[] { type.GetMethod("Invoke") };
			else
				candidateMethods = GetCandidateMethods(type, name);

			var candidates = candidateMethods
                .Select(x => new FunctionMemberData<MethodInfo>(x, x.GetParameters()));

            return _overloadResolver.Resolve(candidates, argTypes);
        }

        public IEnumerable<MethodInfo> GetCandidateMethods(TypeInfo type, string name)
        {
            return type.GetRuntimeMethods()
                .Where(x => x.IsPublic && !x.IsStatic && !x.IsGenericMethod)
                .Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

    }

}
