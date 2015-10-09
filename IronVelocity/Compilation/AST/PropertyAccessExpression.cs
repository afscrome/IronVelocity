using IronVelocity.Binders;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class PropertyAccessExpression : VelocityExpression
    {
        public Expression Target { get; }
        public string Name { get; }

        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.PropertyAccess;

        public PropertyAccessExpression(Expression target, string name, SourceInfo sourceInfo)
        {
            Target = target;
            Name = name;
            SourceInfo = sourceInfo;
        }

        public override Expression Reduce()
        {
            return Expression.Dynamic(
                BinderHelper.Instance.GetGetMemberBinder(Name),
                typeof(object),
                Target
            );
        }

        public PropertyAccessExpression Update(Expression target)
        {
            if (target == Target)
                return this;

            return new PropertyAccessExpression(target, Name, SourceInfo);
        }

    }
}
