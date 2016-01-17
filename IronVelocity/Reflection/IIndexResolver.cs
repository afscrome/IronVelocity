using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Reflection
{
    public interface IIndexResolver
    {
        Expression ReadableIndexer(DynamicMetaObject target, DynamicMetaObject[] args);
        Expression WriteableIndexer(DynamicMetaObject target, DynamicMetaObject[] args);
    }
}
