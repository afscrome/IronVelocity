using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public interface IBinderFactory
    {
        GetMemberBinder GetGetMemberBinder(string memberName);
        SetMemberBinder GetSetMemberBinder(string memberName);
        GetIndexBinder GetGetIndexBinder(int argumentCount);
        SetIndexBinder GetSetIndexBinder(int argumentCount);
        InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount);
        VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation);
        BinaryOperationBinder GetBinaryOperationBinder(ExpressionType type);
    }
}
