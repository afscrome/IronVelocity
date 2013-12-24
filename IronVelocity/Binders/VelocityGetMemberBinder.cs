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
            //If the target doesn't have a value, defer until it does
            if (!target.HasValue)
                return Defer(target);

            if (target.Value == null)
            {
                return new DynamicMetaObject(
                    Constants.NullExpression,
                    BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                );
            }

            Expression result;

            var members = Enumerable.Union<MemberInfo>(
                    target.LimitType.GetProperties(_bindingFlags),
                    target.LimitType.GetFields(_bindingFlags)
                 )
                 .Where(x => x.Name.Equals(Name, System.StringComparison.OrdinalIgnoreCase));

            //If we have more than one match, try a case sensitive match
            if (members.Count() > 1)
            {
                //TODO: Log ambiguity
                Debug.WriteLine(string.Format("Multiple properties found with name '{0}' - looking for exact match", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                members = members.Where(x => x.Name == Name);
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
                if (result.Type.IsValueType)
                    result = Expression.TypeAs(result, typeof(object));
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
