using IronVelocity.Compilation.AST;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Compilation
{
    public class TemporaryLocalReuse : VelocityExpressionVisitor
    {
        private readonly IDictionary<Type, Queue<ParameterExpression>> _avaialbleTemps = new Dictionary<Type, Queue<ParameterExpression>>();
        private readonly IDictionary<ParameterExpression, ParameterExpression> _replacements = new Dictionary<ParameterExpression, ParameterExpression>();

        private int index = 0;
        public IImmutableList<ParameterExpression> TemporaryVariables
            => _avaialbleTemps.SelectMany(x => x.Value).ToImmutableList();

        protected override Expression VisitTemporaryVariableScope(TemporaryVariableScopeExpression node)
        {
            ParameterExpression previousTemp;
            if(!_replacements.TryGetValue(node.Variable, out previousTemp))
            {
                previousTemp = null;
            }

            var newTemp  = GetTemp(node.Variable);
			_replacements[node.Variable] = newTemp;

			var result = Visit(node.Body);

            _replacements[node.Variable] = previousTemp;
            _avaialbleTemps[node.Variable.Type].Enqueue(newTemp);

            return result;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            ParameterExpression replacement;
            if (_replacements.TryGetValue(node, out replacement) && replacement != null)
            {
                return base.VisitParameter(replacement);
            }


            return base.VisitParameter(node);
        }

        private ParameterExpression GetTemp(ParameterExpression variable)
        {
            Queue<ParameterExpression> tempQueue;
            if(!_avaialbleTemps.TryGetValue(variable.Type, out tempQueue))
            {
                tempQueue = new Queue<ParameterExpression>();
				_avaialbleTemps[variable.Type] = tempQueue;
			}

            return tempQueue.Any()
                ? tempQueue.Dequeue()
                : Expression.Parameter(variable.Type, "temp" + index++);
                    
        }
    }
}
