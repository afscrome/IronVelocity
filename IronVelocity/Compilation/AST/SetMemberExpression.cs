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
        public override Type Type { get { return typeof(void); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.SetMember; } }

        public SetMemberExpression(string name, Expression target, Expression value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentOutOfRangeException("name");

            if (target == null)
                throw new ArgumentNullException("target");

            if (value == null)
                throw new ArgumentNullException("target");

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
