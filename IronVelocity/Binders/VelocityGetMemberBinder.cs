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

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            //If the target doesn't have a value, defer until it does
            if (!target.HasValue)
                return Defer(target);

            Expression result;
            if (target.Value == null)
                //TODO: Ideally we wouldn't need this as we would short circuit earlier on so we never even enter the DLR for a null input...
                result = Constants.NullExpression;
            else
            {
                //TODO: Should we allow binding to static methods?
                var members = target.LimitType.GetMember(Name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

                //If we have more than one match, try a case sensitive match
                if (members.Length > 1)
                {
                    //TODO: Log ambiguity
                    Debug.WriteLine(string.Format("Multiple properties found with name '{0}' - looking for exact match", Name, target.LimitType.AssemblyQualifiedName), "Velocity");
                    members = members.Where(x => x.Name == Name).ToArray();
                }
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

                    result = Expression.TypeAs(
                        Expression.MakeMemberAccess(
                            Expression.Convert(target.Expression, member.DeclaringType),
                            member
                        ),
                        typeof(object)
                    );
                }

            }
            return new DynamicMetaObject(
                result,
                BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
            );
        }
    }

}
