using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class IndexResolver : IIndexResolver
    {
        private readonly IOverloadResolver _overloadResolver;
        private readonly IArgumentConverter _argumentConverter = new ArgumentConverter();

        public IndexResolver(IOverloadResolver overloadResolver)
        {
            _overloadResolver = overloadResolver;
        }

        public Expression ReadableIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target.RuntimeType.IsArray)
                return ArrayIndexer(target, args);

            var candidates  = GetCandidateIndexers(target.RuntimeType.GetTypeInfo())
                .Where(x => x.CanRead)
                .Select(x => new FunctionMemberData<PropertyInfo>(x, x.GetIndexParameters()));

            var typeArgs = args.Select(x => x.RuntimeType.GetTypeInfo()).ToArray();

            var result = _overloadResolver.Resolve(candidates, typeArgs);

            if (result == null)
                return null;

            var argExpressions = _overloadResolver.CreateParameterExpressions(result.Parameters, args);

            return Expression.MakeIndex(target.Expression, result.FunctionMember, argExpressions);
        }


        private IEnumerable<PropertyInfo> GetCandidateIndexers(TypeInfo targetType)
        {
            return targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == "Item");
        }


        public Expression WriteableIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return ArrayIndexer(target, args);
        }

        protected virtual Expression ArrayIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var type = target.RuntimeType;
            if (args.All(x => (typeof(int)).IsAssignableFrom(x.RuntimeType)))
            {
                var rank = target.RuntimeType.GetArrayRank();
                return Expression.ArrayAccess(target.Expression, args.Select(x => x.Expression));
            }
            return null;
        }

    }
}
