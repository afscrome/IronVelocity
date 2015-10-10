using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Reflection
{
    public interface IMemberResolver
    {
        //MemberInfo GetMember(string name, Type type, bool caseSensitive);

        //TODO: Move the following elsewhere
        Expression MemberExpression(string name, DynamicMetaObject target, MemberAccessMode mode);
        Expression MemberExpression(string name, Type type, Expression expression, MemberAccessMode mode);
    }
}
