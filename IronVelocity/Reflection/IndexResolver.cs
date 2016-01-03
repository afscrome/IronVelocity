using IronVelocity.Compilation;
using IronVelocity.Compilation.AST;
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

            var candidateIndexers = GetCandidateIndexers(target.RuntimeType.GetTypeInfo())
                .Where(x => x.CanRead);

            return IndexExpression(target, args, candidateIndexers);
        }

        public Expression WriteableIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target.RuntimeType.IsArray)
                return ArrayIndexer(target, args);

            var candidateIndexers = GetCandidateIndexers(target.RuntimeType.GetTypeInfo())
                .Where(x => x.CanWrite);

            return IndexExpression(target, args, candidateIndexers);
        }

        protected virtual Expression ArrayIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var type = target.RuntimeType;
            if (args.All(x => (typeof(int)).IsAssignableFrom(x.RuntimeType)))
            {
                var rank = target.RuntimeType.GetArrayRank();
                var targetExpression = VelocityExpressions.ConvertIfNeeded(target.Expression, type);
                return Expression.ArrayAccess(targetExpression, args.Select(x => x.Expression));
            }
            return null;
        }

        private IEnumerable<PropertyInfo> GetCandidateIndexers(TypeInfo targetType)
        {
            return targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == "Item");
        }

        private Expression IndexExpression(DynamicMetaObject target, DynamicMetaObject[] args, IEnumerable<PropertyInfo> candidateProperties)
        {
            var typeArgs = args.Select(x => x.RuntimeType.GetTypeInfo()).ToArray();

            var candidateData = candidateProperties.Select(x => new FunctionMemberData<PropertyInfo>(x, x.GetIndexParameters()));

            var result = _overloadResolver.Resolve(candidateData, typeArgs);

            if (result == null)
                return null;

            var argExpressions = _overloadResolver.CreateParameterExpressions(result.Parameters, args);

            var targetExpression = VelocityExpressions.ConvertIfNeeded(target.Expression, target.RuntimeType);
            return Expression.MakeIndex(targetExpression, result.FunctionMember, argExpressions);

        }

    }
}
