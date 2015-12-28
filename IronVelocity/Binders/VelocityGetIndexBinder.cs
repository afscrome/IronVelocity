using IronVelocity.Compilation;
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
        public VelocityGetIndexBinder(CallInfo callInfo):
            base(callInfo)
        {

        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));

            if (!target.HasValue || indexes.Any(x => !x.HasValue))
                return Defer(target, indexes);

            // If the target has a null value, then we won't be able to get any fields or properties, so escape early
            // Failure to escape early like this results in an infinite loop
            if (target.Value == null)
                return BinderHelper.NullTargetResult(target,errorSuggestion);

            var arrayRank = target.LimitType.GetArrayRank();

            return arrayRank == 0
                ? CustomIndexer(target, indexes, errorSuggestion)
                : ArrayIndexer(arrayRank, target, indexes, errorSuggestion);
        }

        protected virtual DynamicMetaObject ArrayIndexer(int arrayRank, DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            Expression result;
            var restrictions = BinderHelper.CreateCommonRestrictions(target, indexes);
            if (arrayRank == indexes.Length && indexes.All(x => x.LimitType == typeof(int)))
            {
                result = VelocityExpressions.ConvertIfNeeded(Expression.ArrayAccess(target.Expression, indexes.Select(x => x.Expression).ToArray()), ReturnType);

                return new DynamicMetaObject(result, restrictions);
            }
            else
            {
                if (errorSuggestion != null)
                {
                    result= errorSuggestion.Expression;
                    restrictions = restrictions.Merge(errorSuggestion.Restrictions);
                }
                else
                {
                    result = Constants.VelocityUnresolvableResult;
                }
            }

            return new DynamicMetaObject(result, restrictions);
        }


        protected virtual DynamicMetaObject CustomIndexer(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();

        }
    }
}
