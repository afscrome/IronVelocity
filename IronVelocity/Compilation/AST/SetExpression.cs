using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class SetExpression : VelocityExpression
    {
        public Expression Target { get; private set; }
        public Expression Value { get; private set; }
        public string Name { get; private set; }
        public SetExpression(string name, Expression target, Expression value)
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
            //TODO: allow for reuse of callsites
            return Expression.Dynamic(
                new VelocitySetMemberBinder(Name),
                typeof(void),
                Target,
                Value
            );
        }

        public override Type Type { get { return typeof(void); } }
    }
}
