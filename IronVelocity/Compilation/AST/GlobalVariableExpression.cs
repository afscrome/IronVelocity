using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class GlobalVariableExpression : VelocityExpression
    {
        public string Name { get; }
        public object Value { get; }
        public override Type Type => Value.GetType();
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.GlobalVariable;

        public GlobalVariableExpression(string name, object value)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            Name = name;
            Value = value;
        }

        public override Expression Reduce() => Expression.Constant(Value);
    }
}
