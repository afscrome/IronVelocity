﻿using IronVelocity.Compilation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace IronVelocity.Binders
{
    public class VelocityBinaryOperationBinder : BinaryOperationBinder
    {
        private static IDictionary<Type, ConstructorInfo> _bigIntConstructors = new Dictionary<Type, ConstructorInfo> {
            { typeof(byte), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(int), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(long), typeof(BigInteger).GetConstructor(new[] { typeof(long)})},
            { typeof(sbyte), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(uint), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(ulong), typeof(BigInteger).GetConstructor(new[] { typeof(ulong)})}
        };

        public VelocityBinaryOperationBinder(ExpressionType type)
            : base(type)
        {
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (!arg.HasValue)
                Defer(arg);

            switch (Operation)
            {
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    return MathOperation(target, arg);
                case ExpressionType.And:
                case ExpressionType.Or:
                    return LogicalOperation(target, arg);
                default:
                    throw new InvalidOperationException();
            }

        }


        private DynamicMetaObject LogicalOperation(DynamicMetaObject target, DynamicMetaObject arg)
        {
            var left = CoerceToBoolean(target);
            var right = CoerceToBoolean(arg);

            Expression expression = null;
            BindingRestrictions restrictions = null;
            if (Operation == ExpressionType.And)
            {
                if (left == null)
                    restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, null);
                else if (right == null)
                    restrictions = BindingRestrictions.GetInstanceRestriction(arg.Expression, null);
                else
                    expression = Expression.AndAlso(left, right);
            }
            else if (Operation == ExpressionType.Or)
            {
                if (left == null)
                {
                    if (right == null)
                    {
                        expression = null;
                        restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                            .Merge(BindingRestrictions.GetInstanceRestriction(arg.Expression, null));
                    }
                    else
                    {
                        expression = right;
                        restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                            .Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.RuntimeType));
                    }
                }
                else
                {
                    if (right == null)
                    {
                        expression = left;
                        restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
                            .Merge(BindingRestrictions.GetInstanceRestriction(arg.Expression, null));
                    }
                    else
                    {
                        expression = Expression.OrElse(left, right);
                    }
                }
            }
            else
                throw new InvalidOperationException();

            if (expression == null)
                expression = Expression.Constant(false);

            if (restrictions == null)
            {
                restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
                        .Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.RuntimeType));
            }

            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(expression, ReturnType),
                    restrictions
                );
        }

        private DynamicMetaObject MathOperation(DynamicMetaObject target, DynamicMetaObject arg)
        {
            var result = ReturnNullIfEitherArgumentIsNull(target, arg);
            if (result != null)
                return result;

            Expression left, right, mainExpression;
            MakeArgumentsCompatible(target, arg, out left, out right);

            if (TypeHelper.IsNumeric(left.Type) && TypeHelper.IsNumeric(right.Type))
            {
                try
                {
                    switch (Operation)
                    {
                        case ExpressionType.Add:
                            mainExpression = Expression.AddChecked(left, right);
                            break;
                        case ExpressionType.Subtract:
                            mainExpression = Expression.SubtractChecked(left, right);
                            break;
                        case ExpressionType.Multiply:
                            mainExpression = Expression.MultiplyChecked(left, right);
                            break;
                        case ExpressionType.Divide:
                            mainExpression = Expression.Divide(left, right);
                            break;
                        case ExpressionType.Modulo:
                            mainExpression = Expression.Modulo(left, right);
                            break;
                        default:
                            throw new InvalidProgramException();
                    }
                }
                catch (InvalidOperationException)
                {
                    mainExpression = null;
                }

                if (mainExpression == null)
                {
                    mainExpression = Expression.Default(this.ReturnType);
                }
                else
                {
                    if (Operation == ExpressionType.Divide || Operation == ExpressionType.Modulo)
                    {
                        if (!TypeHelper.SupportsDivisionByZero(right.Type))
                        {
                            mainExpression = Expression.Condition(
                                    Expression.Equal(right, VelocityExpressions.ConvertIfNeeded(Expression.Constant(0), right.Type)),
                                    Expression.Default(ReturnType),
                                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType)
                                );
                        }
                    }
                    else
                    {
                        mainExpression = AddOverflowHandler(mainExpression, left, right);
                    }
                }
            }
            else
            {
                mainExpression = Expression.Default(ReturnType);
            }

            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
                        .Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.RuntimeType))
                );
        }

        private Expression AddOverflowHandler(Expression main, Expression left, Expression right)
        {
            //If we can't convert either of the inputs to BigIntegers, we can't do any overflow handling
            ConstructorInfo leftConstructor, rightConstructor;
            if (!_bigIntConstructors.TryGetValue(left.Type, out leftConstructor) || !_bigIntConstructors.TryGetValue(right.Type, out rightConstructor))
                return main;

            left = Expression.New(leftConstructor, left);
            right = Expression.New(rightConstructor, right);

            Expression oveflowHandler;
            switch (Operation)
            {
                case ExpressionType.Add:
                    oveflowHandler = Expression.Add(left, right);
                    break;
                case ExpressionType.Subtract:
                    oveflowHandler = Expression.Subtract(left, right);
                    break;
                case ExpressionType.Multiply:
                    oveflowHandler = Expression.Multiply(left, right);
                    break;
                default:
                    throw new InvalidOperationException();
            }


            //Pass the final result into ReduceBigInteger(...) to return a more recognizable primitive
            var overflowFallback = Expression.Convert(
                    Expression.Convert(
                        oveflowHandler,
                        typeof(BigInteger)
                    ),
                    ReturnType,
                    MethodHelpers.ReduceBigIntegerMethodInfo
            );

            return Expression.TryCatch(
                VelocityExpressions.ConvertIfNeeded(main, ReturnType),
                Expression.Catch(typeof(OverflowException),
                    overflowFallback
                )
            );
        }

        private static Expression CoerceToBoolean(DynamicMetaObject value)
        {
            if (value.Value == null)
                return null;
            else if (value.RuntimeType == typeof(bool) || value.RuntimeType == typeof(bool?))
                return VelocityExpressions.ConvertIfNeeded(value);
            else if (value.Expression.Type.IsValueType)
                return Expression.Constant(true);
            else
                return Expression.NotEqual(value.Expression, Expression.Constant(null, value.Expression.Type));
        }

        private static void MakeArgumentsCompatible(DynamicMetaObject leftObject, DynamicMetaObject rightObject, out Expression leftExpression, out Expression rightExpression)
        {
            leftExpression = ReflectionHelper.CanBeImplicitlyConverted(leftObject.RuntimeType, rightObject.RuntimeType)
                ? VelocityExpressions.ConvertIfNeeded(leftObject, rightObject.RuntimeType)
                : VelocityExpressions.ConvertIfNeeded(leftObject);

            rightExpression = ReflectionHelper.CanBeImplicitlyConverted(rightObject.RuntimeType, leftObject.RuntimeType)
                ? VelocityExpressions.ConvertIfNeeded(rightObject, leftObject.RuntimeType)
                : VelocityExpressions.ConvertIfNeeded(rightObject);
        }


        private DynamicMetaObject ReturnNullIfEitherArgumentIsNull(DynamicMetaObject left, DynamicMetaObject right)
        {
            BindingRestrictions restrictions;

            if (left.Value == null)
                restrictions = BindingRestrictions.GetInstanceRestriction(left.Expression, null);
            else if (right.Value == null)
                restrictions = BindingRestrictions.GetInstanceRestriction(right.Expression, null);
            else
                return null;

            return new DynamicMetaObject(
                Expression.Default(ReturnType),
                restrictions
            );
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

    }

}
