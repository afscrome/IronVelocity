using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class GlobalVariableExpression : VelocityExpression
    {
        private readonly Type _type;
        private readonly VariableExpression _variable;

        public string Name { get; private set; }
        public GlobalVariableExpression(VariableExpression variable, Type type)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");

            if (type == null)
                throw new ArgumentNullException("type");

            Name = variable.Name;
            Symbols = variable.Symbols;
            _type = type;
            _variable = variable;
        }

        public override Expression Reduce()
        {
            //Need to reduce to prevent infinite loops in the optimiser visitor.
            return VelocityExpressions.ConvertIfNeeded(_variable.Reduce(), Type);
        }

        public override Type Type { get { return _type; } }
    }
}
