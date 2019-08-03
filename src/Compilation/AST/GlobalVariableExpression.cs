using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class GlobalVariableExpression : VelocityExpression
    {
        public string Name { get; }
        public object Value { get; }
        public override Type Type => Value?.GetType() ?? typeof(object);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.GlobalVariable;

        public GlobalVariableExpression(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            if (value == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Value = value;
        }

        public override Expression Reduce() => VelocityExpressions.ConvertIfNeeded(new VariableExpression(Name), Type);
    }
}
