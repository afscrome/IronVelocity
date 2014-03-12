using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation
{
    public static class VelocityExpressions
    {
        public static Expression BoxIfNeeded(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return expression.Type.IsValueType && expression.Type != typeof(void)
                ? Expression.Convert(expression, typeof(object))
                : expression;
        }

        public static Expression ConvertIfNeeded(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (type == null)
                throw new ArgumentNullException("type");

            return ConvertIfNeeded(expression, expression.Type, type);
        }

        public static Expression ConvertIfNeeded(Expression expression, Type from, Type to)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            
            if (to == null)
                throw new ArgumentNullException("to");
            
            if (from == null)
                throw new ArgumentNullException("from");

            if (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpression)expression).Operand;

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
            if (target == null)
                throw new ArgumentNullException("target");

            if (info == null)
                throw new ArgumentNullException("info");

            var expr = target.Expression;
            return ConvertIfNeeded(expr, target.LimitType, info.ParameterType);
        }

        public static Expression ConvertReturnTypeIfNeeded(DynamicMetaObject target, MemberInfo member)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            if (member == null)
                throw new ArgumentNullException("member");

            var expr = target.Expression;

            return ConvertIfNeeded(expr, member.DeclaringType);
        }

        private static readonly Type _dictionaryType = typeof(RuntimeDictionary);
        private static readonly ConstructorInfo _dictionaryConstructorInfo = _dictionaryType.GetConstructor(new[] { typeof(int) });
        private static readonly MethodInfo _dictionaryAddMemberInfo = _dictionaryType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object) }, null);
        public static Expression Dictionary(IDictionary<string, Expression> input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            Expression dictionaryInit = Expression.New(
                    _dictionaryConstructorInfo,
                    Expression.Constant(input.Count)
                );

            //If we're initalising an empty list, we can just return the list as is, without having to create a block expression
            if (!input.Any())
                return dictionaryInit;

            var dictionary = Expression.Parameter(_dictionaryType, "tempDictionary");
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
