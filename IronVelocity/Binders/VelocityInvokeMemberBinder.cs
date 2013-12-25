using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Binders
{
    public class VelocityInvokeMemberBinder : InvokeMemberBinder
    {
        public VelocityInvokeMemberBinder(String name, CallInfo callInfo)
            : base(name, true, callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            //If any of the Dynamic Meta Objects don't yet have a value, defer until they have values.  Failure to do this may result in an infinite loop
            if (!target.HasValue || args.Any(x => !x.HasValue))
                Defer(target, args);

            // If the target has a null value, then we won't be able to invoke any methods, so escape early
            // Failure to escape early like this do this results in an infinite loop
            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );

            }

            var paramCount = args.Length;

            //TODO: Should we allow binding to static methods?
            var members = target.LimitType.GetMethods(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance)
                .Where(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                .Where(x => x.GetParameters().Length == paramCount) //TODO: Doesn't support params arguments
                .ToArray();


            Expression result;
            if (members.Length == 0)
            {
                Debug.WriteLine(string.Format("Unable to resolve Property '{0}' on type '{1}' - Not Found", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                result = Constants.VelocityUnresolvableResult;
            }
            else if (members.Length > 1)
            {
                Debug.WriteLine(string.Format("Unable to resolve Property '{0}' on type '{1}' - Multiple matches found", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                result = Constants.VelocityAmbigiousMatchResult;
            }
            else
            {
                var member = members[0];

                result = Expression.Call(
                    Expression.Convert(target.Expression, member.DeclaringType),
                    member
                );
            }
            return new DynamicMetaObject(
                result,
                BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
            );

        }
    }
}
