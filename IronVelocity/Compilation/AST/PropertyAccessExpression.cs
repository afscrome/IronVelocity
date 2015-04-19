using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class PropertyAccessExpression : VelocityExpression
    {
        public Expression Target { get; private set; }
        public string Name { get; private set; }

        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.PropertyAccess; } }

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
