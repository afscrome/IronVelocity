using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class GlobalVariableExpression : VelocityExpression
    {
        public string Name { get; private set; }
        public object Value { get; private set; }
        public override Type Type { get { return Value.GetType(); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.GlobalVariable; } }

        public GlobalVariableExpression(string name, object value)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException("name");

            Name = name;
            Value = value;
        }

        public override Expression Reduce()
        {
            return Expression.Constant(Value);
        }

    }
}
