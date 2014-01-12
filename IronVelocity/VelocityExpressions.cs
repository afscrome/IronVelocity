using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity
{
    public static class VelocityExpressions
    {
        public static Expression BoxIfNeeded(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            /*
            if (expression.Type == typeof(void))
                return Expression.Block(
                    expression,
                    Expression.Default(typeof(object))
                );
            */
            return expression.Type.IsValueType && expression.Type != typeof(void)
                ? Expression.Convert(expression, typeof(object))
                : expression;
        }

        public static Expression ConvertIfNeeded(Expression expression, Type type)
        {
            return ConvertIfNeeded(expression, expression.Type, type);
        }

        private static Expression ConvertIfNeeded(Expression expression, Type from, Type to)
        {
            if (to.IsValueType && !from.IsValueType && (from.IsInterface || from == typeof(object)))
                return Expression.Unbox(expression, to);
            if (expression.Type != from)
                expression = Expression.Convert(expression, from);
            if (from != to || expression.Type != to)
                expression = Expression.Convert(expression, to);

            return expression;
        }


        public static Expression ConvertParameterIfNeeded(DynamicMetaObject target, ParameterInfo info)
        {
            var expr = target.Expression;
            return ConvertIfNeeded(expr, target.LimitType, info.ParameterType);
        }

        public static Expression ConvertReturnTypeIfNeeded(DynamicMetaObject target, MemberInfo member)
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
