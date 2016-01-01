using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class IndexResolver : IIndexResolver
    {
        private readonly IMethodResolver _methodResolver;

        public IndexResolver(IMethodResolver methodResolver)
        {
            _methodResolver = methodResolver;
        }

        public Expression ReadableIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return target.RuntimeType.IsArray
                ? ArrayIndexer(target, args)
                : CustomIndexer(target, args);
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

        protected virtual Expression CustomIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            //TODO: Is there a custom indexer name (System.Runtime.CompilerServices.IndexerNameAttribute)
            //TODO: What about the whole hierarchy?

            var argTypes = args.Select(X => X.RuntimeType).ToArray();

            var method = _methodResolver.ResolveMethod(target.RuntimeType.GetTypeInfo(), "get_Item", argTypes);

            if (method == null)
                return null;

            return Expression.Call(target.Expression, method,  args.Select(x => x.Expression));
        }

    }
}
