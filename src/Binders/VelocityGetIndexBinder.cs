using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Binders
{
    public class VelocityGetIndexBinder : GetIndexBinder
    {
        private readonly IIndexResolver _indexerResolver;

        public VelocityGetIndexBinder(int argumentCount, IIndexResolver indexerResolver) :
            base(new CallInfo(argumentCount))
        {
            _indexerResolver = indexerResolver;
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));

            if (!target.HasValue || indexes.Any(x => !x.HasValue))
                return Defer(target, indexes);

            if (target.Value == null)
                return BinderHelper.NullTargetResult(target,errorSuggestion);

            var index = _indexerResolver.ReadableIndexer(target, indexes);
            var restrictions = BinderHelper.CreateCommonRestrictions(target, indexes);

            if (index == null)
                return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);

            return new DynamicMetaObject(VelocityExpressions.ConvertIfNeeded(index, ReturnType), restrictions);
        }
    }
}
