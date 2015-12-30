using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Reflection
{
    public class IndexResolver : IIndexResolver
    {
        public Expression ReadableIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return ArrayIndexer(target, args);
        }

        public Expression WriteableIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return ArrayIndexer(target, args);
        }

        public Expression ArrayIndexer(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var rank = target.RuntimeType.GetArrayRank();
            if (rank >0 && args.All(x => (typeof(int)).IsAssignableFrom(x.RuntimeType)))
            {
                return Expression.ArrayAccess(target.Expression, args.Select(x => x.Expression));
            }
            return null;
        }

    }
}
