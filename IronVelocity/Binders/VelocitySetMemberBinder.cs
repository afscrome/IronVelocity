using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class VelocitySetMemberBinder : SetMemberBinder
    {
        private IMemberResolver _memberResolver;

        public VelocitySetMemberBinder(string name, IMemberResolver memberResolver)
            : base(name, ignoreCase: true)
        {
            _memberResolver = memberResolver;
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (value == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.HasValue || ! value.HasValue)
                return Defer(target, value);

            if (target.Value == null)
                return BinderHelper.NullTargetResult(target, errorSuggestion);
            if (value.Value == null)
                return BinderHelper.SetNullValue(this, value);


            var memberAccess = _memberResolver.MemberExpression(Name, target, MemberAccessMode.Write);

            var restrictions = BinderHelper.CreateCommonRestrictions(target);
            if (memberAccess == null || !memberAccess.Type.IsAssignableFrom(value.RuntimeType))
            {
                BindingEventSource.Log.SetMemberResolutionFailure(Name, target.RuntimeType.FullName, value.RuntimeType.FullName);
                return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);
            }

            var result = VelocityExpressions.BoxIfNeeded(
                Expression.Assign(
                    memberAccess,
                    VelocityExpressions.ConvertIfNeeded(value, memberAccess.Type)
                )
            );

            return new DynamicMetaObject(result, restrictions);
        }
    }
}
