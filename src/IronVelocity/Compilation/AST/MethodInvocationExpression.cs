using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class MethodInvocationExpression : VelocityExpression
    {
        private readonly InvokeMemberBinder _binder;
        public Expression Target { get; }
        public string Name => _binder.Name;
        public IImmutableList<Expression> Arguments { get; }

        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.MethodInvocation;

        public MethodInvocationExpression(Expression target, IImmutableList<Expression> arguments, SourceInfo sourceInfo, InvokeMemberBinder binder)
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

        public MethodInvocationExpression Update(Expression target, IImmutableList<Expression> arguments)
        {
            if (target == Target && arguments == Arguments)
                return this;
            else
                return new MethodInvocationExpression(target, arguments, SourceInfo, _binder);
        }
    }
}
