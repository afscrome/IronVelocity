using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class SetMemberExpression : VelocityExpression
    {
        public Expression Target { get; }
        public Expression Value { get; }
        public string Name { get; }
        public override Type Type => typeof(void);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.SetMember;

        public SetMemberExpression(string name, Expression target, Expression value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (value == null)
                throw new ArgumentNullException(nameof(target));

            Target = target;
            Value = value;
            Name = name;
        }

        public override Expression Reduce()
        {
            return Expression.Dynamic(
                BinderHelper.Instance.GetSetMemberBinder(Name),
                typeof(void),
                Target,
                Value
            );
        }

    }
}
