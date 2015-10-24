using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class MethodInvocationExpression : VelocityExpression
    {
        private readonly InvokeMemberBinder _binder;
        public Expression Target { get; }
        public string Name => _binder.Name;
        public IReadOnlyList<Expression> Arguments { get; }

        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.MethodInvocation;

        public MethodInvocationExpression(Expression target, IReadOnlyList<Expression> arguments, SourceInfo sourceInfo, InvokeMemberBinder binder)
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

        public MethodInvocationExpression Update(Expression target, IReadOnlyList<Expression> arguments)
        {
            if (target == Target && arguments == Arguments)
                return this;
            else
                return new MethodInvocationExpression(target, arguments, SourceInfo, _binder);
        }
    }
}
