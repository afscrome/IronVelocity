using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace IronVelocity.Binders
{
    internal static class BigIntegerHelper
    {
        private static readonly IArgumentConverter _argumentConverter = new ArgumentConverter();
        private static IDictionary<Type, ConstructorInfo> _bigIntConstructors = new Dictionary<Type, ConstructorInfo> {
            { typeof(byte), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(int), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(long), typeof(BigInteger).GetConstructor(new[] { typeof(long)})},
            { typeof(sbyte), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(uint), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(ulong), typeof(BigInteger).GetConstructor(new[] { typeof(ulong)})}
        };

        public static Expression ConvertToBigInteger(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression.Type == typeof(BigInteger))
                return expression;

            var unary = expression as UnaryExpression;
            if (unary != null && unary.NodeType == ExpressionType.Convert && TypeHelper.IsInteger(unary.Operand.Type))
                expression = unary.Operand;

            ConstructorInfo constructor;
            if (_bigIntConstructors.TryGetValue(expression.Type, out constructor))
                return Expression.New(constructor, expression);
            else
                throw new NotSupportedException();
        }




    }
}
