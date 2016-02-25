using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Binders
{
    public class VelocityGetMemberBinder : GetMemberBinder
    {
        private readonly IMemberResolver _memberResolver;

        public VelocityGetMemberBinder(string name, IMemberResolver memberResolver)
            : base(name, ignoreCase: true)
        {
            _memberResolver = memberResolver;
        }


        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.HasValue)
                return Defer(target);

            if (target.Value == null)
                return BinderHelper.NullTargetResult(target, errorSuggestion);

            Expression result = null;
            var restrictions = BinderHelper.CreateCommonRestrictions(target);
            try
            {
                result = _memberResolver.MemberExpression(Name, target, MemberAccessMode.Read);
            }
            catch (AmbiguousMatchException)
            {
                BindingEventSource.Log.GetMemberResolutionAmbiguous(Name, target.RuntimeType.FullName);
                return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);
            }


            if (result == null)
            {
                BindingEventSource.Log.GetMemberResolutionFailure(Name, target.RuntimeType.FullName);
                return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);
            }

            return new DynamicMetaObject(
                VelocityExpressions.ConvertIfNeeded(result, ReturnType),
                restrictions
            );
        }
    }

}
