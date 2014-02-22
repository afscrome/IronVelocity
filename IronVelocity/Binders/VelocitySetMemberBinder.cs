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
    public class VelocitySetMemberBinder : SetMemberBinder
    {
        public VelocitySetMemberBinder(string name)
            : base(name, ignoreCase: true)
        {
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            if (value == null)
                throw new ArgumentNullException("target");

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

            var result = ReflectionHelper.MemberExpression(Name, target);

            if (result != null && result.Type.IsAssignableFrom(value.RuntimeType))
                result = VelocityExpressions.BoxIfNeeded(
                    Expression.Assign(
                        result,
                        VelocityExpressions.ConvertIfNeeded(value.Expression, value.RuntimeType, result.Type)
                    )
                );
            else
                result = Constants.VelocityUnresolvableResult;

            return new DynamicMetaObject(
                    result,
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
                );
        }
    }
}
