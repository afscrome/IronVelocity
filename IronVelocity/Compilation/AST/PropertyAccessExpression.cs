using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class PropertyAccessExpression : VelocityExpression
    {
        private readonly Expression _staticExpression;
        private readonly Type _type = typeof(object);

        public Expression Target { get; private set; }
        public string Name { get; private set; }
        public override Type Type { get { return _type; } }

        public PropertyAccessExpression(Expression target, string name, SymbolInformation symbolInformation)
        {
            Target = target;
            Name = name;
            Symbols = symbolInformation;

            if (StaticGlobalVisitor.IsConstantType(Target))
            {
                _staticExpression = ReflectionHelper.MemberExpression(Name, Target.Type, Target)
                    ?? Constants.NullExpression;
                _type = _staticExpression.Type;
            }
        }

        public override Expression Reduce()
        {
            return _staticExpression
                ?? Expression.Dynamic(
                    new VelocityGetMemberBinder(Name),
                    typeof(object),
                    Target
                );
        }

        public PropertyAccessExpression Update(Expression target)
        {
            if (target == Target)
                return this;

            return new PropertyAccessExpression(target, Name, Symbols);
        }

    }
}
