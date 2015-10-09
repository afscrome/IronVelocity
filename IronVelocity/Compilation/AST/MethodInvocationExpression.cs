using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class MethodInvocationExpression : VelocityExpression
    {
        public Expression Target { get; }
        public string Name { get; }
        public IReadOnlyList<Expression> Arguments { get; }

        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.MethodInvocation; } }

        public MethodInvocationExpression(Expression target, string name, IReadOnlyList<Expression> arguments, SourceInfo sourceInfo)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            if (String.IsNullOrEmpty(name))
                throw new ArgumentOutOfRangeException("name");

            if (arguments == null)
                throw new ArgumentOutOfRangeException("arguments");

            Target = target;
            Name = name;
            Arguments = arguments;
            SourceInfo = sourceInfo;
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
                BinderHelper.Instance.GetInvokeMemberBinder(Name, Arguments.Count),
                typeof(object),
                args
            );
        }

        public MethodInvocationExpression Update(Expression target, IReadOnlyList<Expression> arguments)
        {
            if (target == Target && arguments == Arguments)
                return this;
            else
                return new MethodInvocationExpression(target, Name, arguments, SourceInfo);
        }

    }
}
