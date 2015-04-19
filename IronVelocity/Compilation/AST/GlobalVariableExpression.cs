using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class GlobalVariableExpression : VelocityExpression
    {
        private readonly Type _type;
        private readonly VariableExpression _variable;

        public string Name { get; private set; }

        public override Type Type { get { return _type; } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.GlobalVariable; } }

        public GlobalVariableExpression(VariableExpression variable, Type type)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");

            if (type == null)
                throw new ArgumentNullException("type");

            Name = variable.Name;
            SourceInfo = variable.SourceInfo;
            _type = type;
            _variable = variable;
        }

        public override Expression Reduce()
        {
            //Need to reduce to prevent infinite loops in the optimiser visitor.
            return VelocityExpressions.ConvertIfNeeded(_variable.Reduce(), Type);
        }

    }
}
