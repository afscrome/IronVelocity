using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class IndexInvocationExpression : VelocityExpression
    {
        private readonly GetIndexBinder _binder;
        public Expression Target { get; }
        public IImmutableList<Expression> Arguments { get; }

        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.IndexInvocation;

        public IndexInvocationExpression(Expression target, IImmutableList<Expression> arguments, SourceInfo sourceInfo, GetIndexBinder binder)
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
            var builder = ImmutableArray.CreateBuilder<Expression>(Arguments.Count + 1);
            builder.Add(Target);
            builder.AddRange(Arguments);

            return Expression.Dynamic(
                _binder,
                _binder.ReturnType,
                builder.ToImmutable()
            );
        }
    }
}
