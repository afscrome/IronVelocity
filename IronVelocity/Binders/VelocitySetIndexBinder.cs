using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class VelocitySetIndexBinder : SetIndexBinder
    {
        private readonly IIndexResolver _indexerResolver;

        public VelocitySetIndexBinder(int argCount, IIndexResolver indexerResolver)
            : base(new CallInfo(argCount))
        {
            _indexerResolver = indexerResolver;
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));

            if (!target.HasValue || !value.HasValue || indexes.Any(x => !x.HasValue))
                return Defer(target, indexes.Concat(new[] { value }).ToArray());

            if (target.Value == null)
                return BinderHelper.NullTargetResult(target, errorSuggestion);
            if (value.Value == null)
                return BinderHelper.SetNullValue(this, value);

            var restrictions = BinderHelper.CreateCommonRestrictions(target, indexes)
                .Merge(BinderHelper.CreateCommonRestrictions(value));

            var index = _indexerResolver.WriteableIndexer(target, indexes);

            if (index == null || !index.Type.IsAssignableFrom(value.RuntimeType))
                return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);

            var assignment = Expression.Assign(index, VelocityExpressions.ConvertIfNeeded(value.Expression, index.Type));

            return new DynamicMetaObject(VelocityExpressions.ConvertIfNeeded(assignment, ReturnType), restrictions);
        }
    }
}
