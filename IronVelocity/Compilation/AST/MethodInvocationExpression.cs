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
        private readonly Expression _staticExpression;
        private readonly Type _type = typeof(object);

        public Expression Target { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<Expression> Arguments { get; private set; }
        public override Type Type { get { return _type; } } 

        public MethodInvocationExpression(Expression target, string name, IReadOnlyList<Expression> arguments, SymbolInformation symbols)
        {
            Target = target;
            Name = name;
            Arguments = arguments;
            Symbols = symbols;

            if (StaticGlobalVisitor.IsConstantType(Target) && Arguments.All(StaticGlobalVisitor.IsConstantType))
            {
                var method = ReflectionHelper.ResolveMethod(target.Type, Name, GetArgumentTypes(Arguments));

                _staticExpression = method == null
                    ? Constants.NullExpression
                    : ReflectionHelper.ConvertMethodParameters(method, target, Arguments.Select(x => new DynamicMetaObject(x, BindingRestrictions.Empty)).ToArray());

                _type = _staticExpression.Type;
            }
        
        }

        public override Expression Reduce()
        {
            if (_staticExpression != null)
                return _staticExpression;

            var args = new Expression[Arguments.Count + 1];
            args[0] = Target;

            for (int i = 0; i < Arguments.Count; i++)
            {
                args[i + 1] = Arguments[i];
            }

            //TODO: allow for reuse of callsites
            return Expression.Dynamic(
                new VelocityInvokeMemberBinder(Name, new CallInfo(Arguments.Count)),
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
