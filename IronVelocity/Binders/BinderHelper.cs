using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    internal static class BinderHelper
    {
        public static BindingRestrictions CreateCommonRestrictions(DynamicMetaObject value)
        {
            var valueRestriction = value.HasValue && value.Value == null
                    ? BindingRestrictions.GetInstanceRestriction(value.Expression, null)
                    : BindingRestrictions.GetTypeRestriction(value.Expression, value.LimitType);

            return valueRestriction.Merge(value.Restrictions);
        }

        public static BindingRestrictions CreateCommonRestrictions(DynamicMetaObject target, params DynamicMetaObject[] args)
        {
            var restrictions = CreateCommonRestrictions(target);

            foreach (var arg in args)
            {
                restrictions = restrictions.Merge(CreateCommonRestrictions(arg));
            }

            return restrictions;
        }


        public static DynamicMetaObject NullTargetResult(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            return errorSuggestion
                ?? new DynamicMetaObject(
                    Constants.VelocityUnresolvableResult,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
        }

        private static readonly DynamicMetaObject _nullResult = DynamicMetaObject.Create(null, Constants.VelocityUnresolvableResult);
        public static DynamicMetaObject UnresolveableResult(BindingRestrictions restrictions, DynamicMetaObject errorSuggestion)
        {
            if (errorSuggestion == null)
                return _nullResult;

            restrictions = restrictions.Merge(errorSuggestion.Restrictions);

            return new DynamicMetaObject(errorSuggestion.Expression, restrictions);
        }

        public static DynamicMetaObject SetNullValue(DynamicMetaObjectBinder binder, DynamicMetaObject value)
        {
            var restriction = BindingRestrictions.GetInstanceRestriction(value.Expression, null);
            return new DynamicMetaObject(Expression.Default(binder.ReturnType), restriction); 
        }

    }
}
