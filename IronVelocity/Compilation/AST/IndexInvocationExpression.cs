using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class IndexInvocationExpression : VelocityExpression
    {
        private readonly GetIndexBinder _binder;
        public Expression Target { get; }
        public IReadOnlyList<Expression> Arguments { get; }

        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.IndexInvocation;

        public IndexInvocationExpression(Expression target, IReadOnlyList<Expression> arguments, SourceInfo sourceInfo, GetIndexBinder binder)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (arguments == null)
                throw new ArgumentOutOfRangeException(nameof(arguments));

            Target = target;
            Arguments = arguments;
            SourceInfo = sourceInfo;
            _binder = binder;
        }

        public override Expression Reduce()
        {
            var args = new Expression[Arguments.Count + 1];
            args[0] = Target;

            for (int i = 0; i < Arguments.Count; i++)
            {
                args[i + 1] = Arguments[i];
            }

            return Expression.Dynamic(
                _binder,
                _binder.ReturnType,
                args
            );
        }
    }
}
