using IronVelocity.Binders;
using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class SetMemberExpression : VelocityExpression
    {
        private readonly SetMemberBinder _binder;

        public Expression Target { get; }
        public Expression Value { get; }
        public string Name => _binder.Name;
        public override Type Type => typeof(void);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.SetMember;

        public SetMemberExpression(Expression target, Expression value, SetMemberBinder binder)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (value == null)
                throw new ArgumentNullException(nameof(target));

            Target = target;
            Value = value;
            _binder = binder;
        }

        public override Expression Reduce()
        {
            return Expression.Dynamic(
                _binder,
                typeof(void),
                Target,
                Value
            );
        }

    }
}
