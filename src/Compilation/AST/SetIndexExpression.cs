using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class SetIndexExpression : VelocityExpression
    {
        private readonly SetIndexBinder _binder;

        public Expression Target { get; }
        public Expression Value { get; }
        public IImmutableList<Expression> Arguments { get; }


        public override Type Type => typeof(void);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.SetIndex;

        public SetIndexExpression(Expression target, Expression value, IImmutableList<Expression> arguments, SetIndexBinder binder)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (value == null)
                throw new ArgumentNullException(nameof(target));

            if (arguments == null)
                throw new ArgumentOutOfRangeException(nameof(arguments));

            Target = target;
            Arguments = arguments;
            Value = value;
            _binder = binder;
        }

        public override Expression Reduce()
        {
            var builder = ImmutableArray.CreateBuilder<Expression>(Arguments.Count + 2);
            builder.Add(Target);
            builder.AddRange(Arguments);
            builder.Add(Value);

            return Expression.Dynamic(
                _binder,
                typeof(void),
                builder.ToImmutable()
            );
        }
    }
}
