using IronVelocity.Compilation;
using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class VelocityGetMemberBinder : GetMemberBinder
    {
        public VelocityGetMemberBinder(string name)
            : base(name, ignoreCase: true)
        {
        }


        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            //If any of the Dynamic Meta Objects don't yet have a value, defer until they have values.  Failure to do this may result in an infinite loop
            if (!target.HasValue)
                return Defer(target);

            // If the target has a null value, then we won't be able to get any fields or properties, so escape early
            // Failure to escape early like this do this results in an infinite loop
            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
            }

            var result = ReflectionHelper.MemberExpression(Name, target);

            if (result == null)
            {
                var invokeBinder = new VelocityInvokeMemberBinder(Name, new CallInfo(0));
                return invokeBinder.FallbackInvokeMember(target, new DynamicMetaObject[0], errorSuggestion);
            }
            else
            {

                //Dynamic return type is object, but primitives are not objects
                // DLR does not handle boxing to make primitives objects, so do it ourselves
                result = VelocityExpressions.BoxIfNeeded(result);

                return new DynamicMetaObject(
                    result,
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
                );
            }
        }
    }

}
