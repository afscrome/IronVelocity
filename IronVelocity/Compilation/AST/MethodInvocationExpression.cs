using IronVelocity.Binders;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class MethodInvocationExpression : VelocityExpression
    {
        public Expression Target { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<Expression> Arguments { get; private set; }

        public MethodInvocationExpression(Expression target, string name, IReadOnlyList<Expression> arguments, SymbolInformation symbols)
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
            Symbols = symbols;
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
            return new MethodInvocationExpression(target, Name, arguments, Symbols);
        }

        private static Type[] GetArgumentTypes(IReadOnlyList<Expression> expressions)
        {
            var types = new Type[expressions.Count];

            for (int i = 0; i < expressions.Count; i++)
            {
                types[i] = expressions[i].Type;
            }

            return types;
        }

    }
}
