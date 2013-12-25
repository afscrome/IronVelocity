using IronVelocity;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VelocityExpressionTree.Binders
{
    public class VelocityGetMemberBinder : GetMemberBinder
    {
        public VelocityGetMemberBinder(string name)
            : base(name, ignoreCase: true)
        {
        }

        //TODO: Should we allow binding to static members?
        private static readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Instance;

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
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

            Expression result;

            //Get properties and fields with the same name (case insensitive)
            var members = Enumerable.Union<MemberInfo>(
                    target.LimitType.GetProperties(_bindingFlags),
                    target.LimitType.GetFields(_bindingFlags)
                 )
                 .Where(x => x.Name.Equals(Name, System.StringComparison.OrdinalIgnoreCase));

            // Velocity is case insensitive but .net is case sensitive.  This means we may 
            // In such an event, do a case sensitive match
            if (members.Count() > 1)
            {
                Debug.WriteLine(string.Format("Multiple properties found with name '{0}' on type '{1}' - looking for exact match", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                members = members.Where(x => x.Name.Equals(Name, System.StringComparison.Ordinal));
            }


            if (!members.Any())
            {
                Debug.WriteLine(string.Format("Unable to resolve Property '{0}' on type '{1}' - Not Found", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                result = Constants.VelocityUnresolvableResult;
            }
            else if (members.Count() > 1)
            {
                Debug.WriteLine(string.Format("Unable to resolve Property '{0}' on type '{1}' - Multiple matches found", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                result = Constants.VelocityAmbigiousMatchResult;
            }
            else
            {
                var member = members.Single();
                result = Expression.MakeMemberAccess(
                        Expression.Convert(target.Expression, member.DeclaringType),
                        member
                    );

                //DLR doesn't like value types, so box if it's a value type
                result = VelocityExpressions.BoxIfNeeded(result);
                //var expressionType = memberType.IsValueType ? ExpressionType.Convert : ExpressionType.TypeAs;
                //result = Expression.MakeUnary(expressionType, memberExpression, memberType);
            }

            return new DynamicMetaObject(
                result,
                BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
            );
        }
    }

}
