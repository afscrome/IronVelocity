using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.AST
{
    public class VariableExpression : VelocityExpression
    {
        public string Name { get; }
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.Variable;

        public VariableExpression(string name)
        {
            Name = name;
        }

        private static readonly PropertyInfo _indexerProperty = typeof(VelocityContext).GetProperty("Item", typeof(Expression), new[] { typeof(string) });

        public override Expression Reduce()
            => Expression.MakeIndex(Constants.InputParameter, _indexerProperty, new[] { Expression.Constant(Name) });
    }
}
