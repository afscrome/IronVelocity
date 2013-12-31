using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace IronVelocity
{
    public static class VelocityExpressions
    {
        public static Expression BoxIfNeeded(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return expression.Type.IsValueType
                ? Expression.Convert(expression, typeof(object))
                : expression;
        }

        public static Expression ConvertIfNeeded(Expression expression, Type type)
        {
            if (type.IsValueType && !expression.Type.IsValueType)
                return Expression.Unbox(expression, type);

            return expression.Type == type
                ? expression
                : Expression.Convert(expression, type);
        }

        public static Expression ConvertIfNeeded(DynamicMetaObject target, MemberInfo member)
        {
            var expr = target.Expression;

            return ConvertIfNeeded(expr, member.DeclaringType);
        }

        private static readonly ConstructorInfo _dictionaryConstructorInfo = typeof(Dictionary<string, object>).GetConstructor(new[] { typeof(int), typeof(IEqualityComparer<string>) });
        private static readonly MethodInfo _dictionaryAddMemberInfo = typeof(Dictionary<string, object>).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object) }, null);
        public static Expression Dictionary(IDictionary<string, Expression> input)
        {

            Expression dictionaryInit = Expression.New(
                    _dictionaryConstructorInfo,
                    Expression.Constant(input.Count),
                    Expression.Constant(StringComparer.OrdinalIgnoreCase)
                );

            //If we're initalising an empty list, we can just return the list as is, without having to create a block expression
            if (!input.Any())
                return dictionaryInit;

            var dictionary = Expression.Parameter(typeof(Dictionary<string, object>), "dictionary");
            dictionaryInit = Expression.Assign(dictionary, dictionaryInit);

            var valuesInit = input.Select(x => Expression.Call(
                        dictionary,
                        _dictionaryAddMemberInfo,
                        Expression.Constant(x.Key),
                        Expression.Convert(x.Value, typeof(object))
                    )
                ).OfType<Expression>();


            return Expression.Block(
                new[] { dictionary },
                Enumerable.Union(new[] { dictionaryInit }, valuesInit).Union(new[] { dictionary })
            );

        }


    }
}
