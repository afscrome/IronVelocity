using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public IImmutableDictionary<Expression, Expression> Values { get; }
        public override Type Type => typeof(RuntimeDictionary);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.Dictionary;


        public DictionaryExpression(IImmutableDictionary<Expression, Expression> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            Values = values;
        }


        public DictionaryExpression(IImmutableDictionary<string, Expression> values)
        {
            if (values == null)
            {
                Values = ImmutableDictionary<Expression, Expression>.Empty;
            }
            else
            {
                var builder = ImmutableDictionary.CreateBuilder<Expression, Expression>();

                foreach (var item in values)
                {
                    builder.Add(Expression.Constant(item.Key), item.Value);
                }

                Values = builder.ToImmutable();
            }
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
                x.Key,
                VelocityExpressions.ConvertIfNeeded(x.Value, typeof(object))
            ));

            return Expression.ListInit(dictionaryInit, initializers);
        }

        public DictionaryExpression Update(IImmutableDictionary<string, Expression> values)
        {
            return Values == values
                ? this
                : new DictionaryExpression(values);
        }

    }
}
