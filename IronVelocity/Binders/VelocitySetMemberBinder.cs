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

            //If any of the Dynamic Meta Objects don't yet have a value, defer until they have values.  Failure to do this may result in an infinite loop
            if (!target.HasValue)
                return Defer(target);

            // If the target has a null value, then we won't be able to make an assignment
            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
            }

            var result = _memberResolver.MemberExpression(Name, target, Reflection.MemberAccessMode.Write);

            if (result != null && result.Type.IsAssignableFrom(value.RuntimeType))
                result = VelocityExpressions.BoxIfNeeded(
                    Expression.Assign(
                        result,
                        VelocityExpressions.ConvertIfNeeded(value, result.Type)
                    )
                );
            else
            {
                BindingEventSource.Log.SetMemberResolutionFailure(Name, target.RuntimeType.FullName, value.RuntimeType.FullName);
                result = Constants.VelocityUnresolvableResult;
            }

            return new DynamicMetaObject(
                    result,
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
                );
        }
    }
}
