using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace IronVelocity.Binders
{
    public static class BinaryOperationHelper
    {
        private static IDictionary<Type, ConstructorInfo> _bigIntConstructors = new Dictionary<Type, ConstructorInfo> {
            { typeof(byte), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(int), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(long), typeof(BigInteger).GetConstructor(new[] { typeof(long)})},
            { typeof(sbyte), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(uint), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(ulong), typeof(BigInteger).GetConstructor(new[] { typeof(ulong)})}
        };


        public static void MakeArgumentsCompatible(DynamicMetaObject leftObject, DynamicMetaObject rightObject, out Expression leftExpression, out Expression rightExpression)
        {
            if (leftObject == null)
                throw new ArgumentNullException("leftObject");

            if (rightObject == null)
                throw new ArgumentNullException("rightObject");

            var leftType = leftObject.RuntimeType ?? typeof(object);
            var rightType = rightObject.RuntimeType ?? typeof(object);

            leftExpression = VelocityExpressions.ConvertIfNeeded(leftObject);
            rightExpression = VelocityExpressions.ConvertIfNeeded(rightObject);

            if(ReflectionHelper.CanBeImplicitlyConverted(leftType, rightType))
            {
                leftExpression = VelocityExpressions.ConvertIfNeeded(leftObject, rightType);
            }
            else if (ReflectionHelper.CanBeImplicitlyConverted(rightType, leftType))
            {
                rightExpression = VelocityExpressions.ConvertIfNeeded(rightObject, leftType);
            }
            else if (leftType == typeof(string) && (rightType == typeof(char) || rightType.IsEnum))
            {
                rightExpression = Expression.Call(rightExpression, MethodHelpers.ToStringMethodInfo);
            }
            else if (rightType == typeof(string) && (leftType == typeof(char) || leftType.IsEnum))
            {
                leftExpression = Expression.Call(leftExpression, MethodHelpers.ToStringMethodInfo);
            }
            else if (leftType != rightType && TypeHelper.IsInteger(leftType) && TypeHelper.IsInteger(rightType))
            {
                leftExpression = ConvertToBigInteger(leftExpression);
                rightExpression = ConvertToBigInteger(rightExpression);
            }
        }


        public static Expression ConvertToBigInteger(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

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


        public static object ReduceBigInteger(BigInteger value, bool returnSignedIntegers)
        {
            if (returnSignedIntegers)
            {
                if (value >= uint.MinValue && value <= uint.MaxValue)
                    return (uint)value;
                if (value >= ulong.MinValue && value <= ulong.MaxValue)
                    return (ulong)value;
            }
            else
            {
                if (value >= int.MinValue && value <= int.MaxValue)
                    return (int)value;
                if (value >= long.MinValue && value <= long.MaxValue)
                    return (long)value;
            }

            return (float)value;
        }


        public static BindingRestrictions DeduceArgumentRestrictions(DynamicMetaObject value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Value != null)
                return BindingRestrictions.GetTypeRestriction(value.Expression, value.RuntimeType);
            else
                return BindingRestrictions.GetInstanceRestriction(value.Expression, null);
        }

        public static BindingRestrictions GetNotValueTypeRestrictions(Expression expression)
        {
            var restrictionExpression = Expression.Not(Expression.TypeIs(expression, typeof(ValueType)));
            return BindingRestrictions.GetExpressionRestriction(restrictionExpression);
        }
    }
}
