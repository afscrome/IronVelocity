using IronVelocity;
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

            var members = target.LimitType.GetMember(Name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance);

            //If we have more than one match, try a case sensitive match
            if (members.Length > 1)
            {
                //TODO: Log ambiguity
                members = members.Where(x => x.Name == Name).ToArray();
            }
            Expression result;
            if (members.Length == 0)
                result = Constants.VelocityUnresolvableResult;
            else if (members.Length > 1)
                result = Constants.VelocityAmbigiousMatchResult;
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
            return new DynamicMetaObject(
                result,
                BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
            );
        }
    }

}
