﻿using IronVelocity.Binders;
using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Reflection
{
    public class ArgumentConverter : IArgumentConverter
    {
        /// <summary>
        /// A list of implicit numeric conversions as defined in section 6.1.2 of the C# 5.0 spec
        /// The value array indicates types which the Key type can be implicitly converted to.
        /// </summary>
        private static readonly IDictionary<Type, Type[]> _implicitNumericConversions = new Dictionary<Type, Type[]>(){
                    { typeof(sbyte), new[]{typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(byte), new[]{typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(short), new[]{typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(ushort), new[]{typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(int), new[]{typeof(long), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(uint), new[]{typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(long), new[]{typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(ulong), new[]{typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(char), new[]{typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(float), new[]{typeof(double)}},
                };

        public bool CanBeConverted(Type from, Type to) => GetConverter(from, to) != null;

        private static readonly IExpressionConverter _implicitConverter = new ImplicitExpressionConverter();
        public IExpressionConverter GetConverter(Type from, Type to)
        {
            //from may be null, but to may not be
            if (to == null)
                throw new ArgumentNullException(nameof(to));

			if (from == null)
			{
				return TypeHelper.IsNullableType(to)
					? _implicitConverter
					: null;
			}

            if (to.IsAssignableFrom(from))
                return _implicitConverter;


            /*
            if (from.IsEnum)
                return GetConverter(Enum.GetUnderlyingType(from), to);
            */

            if (from.IsGenericType && from.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                from = from.GenericTypeArguments[0];
            }

            if (from.IsPrimitive)
            {
                if (to.IsGenericType && to.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    to = to.GenericTypeArguments[0];
                }

                Type[] supportedConversions;
                if (_implicitNumericConversions.TryGetValue(from, out supportedConversions))
                    return supportedConversions.Contains(to)
                        ? _implicitConverter
                        : null;
            }

            return null;
        }

        public void MakeBinaryOperandsCompatible(Type leftType, Type rightType, ref Expression leftExpression, ref Expression rightExpression)
        {
            if (CanBeConverted(leftType, rightType))
            {
                leftExpression = VelocityExpressions.ConvertIfNeeded(leftExpression, rightType);
            }
            else if (CanBeConverted(rightType, leftType))
            {
                rightExpression = VelocityExpressions.ConvertIfNeeded(rightExpression, leftType);
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
                leftExpression = BigIntegerHelper.ConvertToBigInteger(leftExpression);
                rightExpression = BigIntegerHelper.ConvertToBigInteger(rightExpression);
            }
        }

    }
}
