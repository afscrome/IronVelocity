using IronVelocity.Binders;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class PropertyAccessExpression : VelocityExpression
    {
        public Expression Target { get; }
        public string Name { get; }
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.PropertyAccess;

        private readonly GetMemberBinder _binder;

        public PropertyAccessExpression(Expression target, string name, SourceInfo sourceInfo, GetMemberBinder binder)
        {
            Target = target;
            Name = name;
            SourceInfo = sourceInfo;
            _binder = binder;
        }

        public override Expression Reduce()
        {
            return Expression.Dynamic(
                _binder,
                _binder.ReturnType,
                Target
            );
        }

        public PropertyAccessExpression Update(Expression target)
        {
            if (target == Target)
                return this;

            return new PropertyAccessExpression(target, Name, SourceInfo, _binder);
        }

    }
}
