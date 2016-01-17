using System;
using System.Collections.Generic;
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
        public IReadOnlyList<Expression> Arguments { get; }


        public override Type Type => typeof(void);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.SetIndex;

        public SetIndexExpression(Expression target, Expression value, IReadOnlyList<Expression> arguments, SetIndexBinder binder)
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
            var args = new Expression[Arguments.Count + 2];
            args[0] = Target;

            for (int i = 0; i < Arguments.Count; i++)
            {
                args[i + 1] = Arguments[i];
            }
            args[args.Length - 1] = Value;

            return Expression.Dynamic(
                _binder,
                typeof(void),
                args
            );
        }
    }
}
