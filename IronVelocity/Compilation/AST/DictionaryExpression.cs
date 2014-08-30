using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.AST
{
    public class DictionaryExpression : VelocityExpression
    {
        private static readonly Type _dictionaryType = typeof(RuntimeDictionary);
        private static readonly ConstructorInfo _dictionaryConstructorInfo = _dictionaryType.GetConstructor(new[] { typeof(int) });
        private static readonly MethodInfo _dictionaryAddMemberInfo = _dictionaryType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object) }, null);

        public IReadOnlyDictionary<string, Expression> Values { get; private set; }
        public override Type Type { get { return typeof(RuntimeDictionary); } }

        public DictionaryExpression(IReadOnlyDictionary<string, Expression> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            Values = values;
        }

        public override Expression Reduce()
        {
            var dictionaryInit = Expression.New(
                    _dictionaryConstructorInfo,
                    Expression.Constant(Values.Count)
                );

            if (!Values.Any())
                return dictionaryInit;

            var initializers = Values.Select(x => Expression.ElementInit(
                _dictionaryAddMemberInfo,
                Expression.Constant(x.Key),
                VelocityExpressions.ConvertIfNeeded(x.Value, typeof(object))
            ));

            return Expression.ListInit(dictionaryInit, initializers);
        }
       
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            bool changed = true;

            var visitedValues = new Dictionary<string, Expression>(Values.Count);
            foreach (var pair in Values)
            {
                var value = visitor.Visit(pair.Value);
                if (value != pair.Value)
                {
                    changed = true;
                }
                visitedValues[pair.Key] = value;
            }

            var result = changed 
                ? new DictionaryExpression(visitedValues)
                : this;

            return result;
        }
        
    }
}
