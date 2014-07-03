using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            var leftType = leftObject.RuntimeType ?? typeof(object);
            var rightType = rightObject.RuntimeType ?? typeof(object);

            leftExpression = ReflectionHelper.CanBeImplicitlyConverted(leftType, rightType)
                ? VelocityExpressions.ConvertIfNeeded(leftObject, rightType)
                : VelocityExpressions.ConvertIfNeeded(leftObject);

            rightExpression = ReflectionHelper.CanBeImplicitlyConverted(rightType, leftType)
                ? VelocityExpressions.ConvertIfNeeded(rightObject, leftType)
                : VelocityExpressions.ConvertIfNeeded(rightObject);


            if (leftType == typeof(string) && (rightType == typeof(char) || rightType.IsEnum))
                rightExpression = Expression.Call(rightExpression, MethodHelpers.ToStringMethodInfo);
            else if (rightType == typeof(string) && (leftType == typeof(char) || leftType.IsEnum))
                leftExpression = Expression.Call(leftExpression, MethodHelpers.ToStringMethodInfo);
            else if (leftType != rightType && TypeHelper.IsInteger(leftType) && TypeHelper.IsInteger(rightType))
            {
                var leftBigInt = ConvertToBigIntegerIfPossible(leftExpression);
                var rightBigInt = ConvertToBigIntegerIfPossible(rightExpression);
                if (leftBigInt.Type == rightBigInt.Type)
                {
                    leftExpression = leftBigInt;
                    rightExpression = rightBigInt;
                }
            }
        }


        public static Expression ConvertToBigIntegerIfPossible(Expression expression)
        {
            ConstructorInfo constructor;
            if (_bigIntConstructors.TryGetValue(expression.Type, out constructor))
                return Expression.New(constructor, expression);
            else
                return expression;
        }


        public static object ReduceBigInteger(BigInteger value)
        {
            if (value >= int.MinValue && value <= int.MaxValue)
                return (int)value;
            if (value >= long.MinValue && value <= long.MaxValue)
                return (long)value;
            if (value >= ulong.MinValue && value <= ulong.MaxValue)
                return (ulong)value;

            return (float)value;
        }


        public static BindingRestrictions DeduceArgumentRestrictions(DynamicMetaObject value)
        {
            if (value.Value != null)
                return BindingRestrictions.GetTypeRestriction(value.Expression, value.RuntimeType);
            else
                return BindingRestrictions.GetInstanceRestriction(value.Expression, null);
        }
    }
}
