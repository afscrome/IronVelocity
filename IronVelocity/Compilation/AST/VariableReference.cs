using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.AST
{
    public class VariableReference : VelocityExpression
    {
        public string Name { get; private set; }
        public VariableReference(string name)
        {
            Name = name;
        }

        private static readonly PropertyInfo _indexerProperty = typeof(VelocityContext).GetProperty("Item", typeof(Expression), new[] { typeof(string) });
        public override Expression Reduce()
        {
            return Expression.MakeIndex(Constants.InputParameter, _indexerProperty, new[] { Expression.Constant(Name) });
        }
    }
}
