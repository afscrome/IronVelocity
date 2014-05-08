using IronVelocity.Compilation;
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
            if (arg == null)
                throw new ArgumentNullException("arg");

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
                case ExpressionType.Equal:
                    return Compare(target, arg, Expression.Equal);
                case ExpressionType.NotEqual:
                    return Compare(target, arg, Expression.NotEqual);
                case ExpressionType.LessThan:
                    return Compare(target, arg, Expression.LessThan);
                case ExpressionType.LessThanOrEqual:
                    return Compare(target, arg, Expression.LessThanOrEqual);
                case ExpressionType.GreaterThan:
                    return Compare(target, arg, Expression.GreaterThan);
                case ExpressionType.GreaterThanOrEqual:
                    return Compare(target, arg, Expression.GreaterThanOrEqual);
                default:
                    throw new InvalidOperationException();
            }

        }

        private DynamicMetaObject Compare(DynamicMetaObject target, DynamicMetaObject arg, Func<Expression, Expression, Expression> generator)
        {
            Expression left, right, mainExpression;
            MakeArgumentsCompatible(target, arg, out left, out right);

            try
            {
                mainExpression = generator(left, right);
            }
            catch (InvalidOperationException)
            {
                if (generator == Expression.Equal || generator == Expression.NotEqual)
                {
                    mainExpression = generator(
                        VelocityExpressions.ConvertIfNeeded(left, ReturnType),
                        VelocityExpressions.ConvertIfNeeded(right, ReturnType)
                    );
                }
                else
                {
                    mainExpression = Expression.Constant(false);
                }
            }

            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    DeduceArgumentRestrictions(target).Merge(DeduceArgumentRestrictions(arg))
                );

        }

        private DynamicMetaObject Equals(DynamicMetaObject target, DynamicMetaObject arg)
        {
            Expression left, right, mainExpression;
            MakeArgumentsCompatible(target, arg, out left, out right);

            try
            {
                mainExpression = Expression.Equal(left, right);
            }
            catch (InvalidOperationException)
            {
                mainExpression = Expression.Equal(
                    VelocityExpressions.ConvertIfNeeded(left, ReturnType),
                    VelocityExpressions.ConvertIfNeeded(right, ReturnType)
                );
            }
            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    DeduceArgumentRestrictions(target).Merge(DeduceArgumentRestrictions(arg))
                );
        }

        private static BindingRestrictions DeduceArgumentRestrictions(DynamicMetaObject value)
        {
            if (value.Value != null)
                return BindingRestrictions.GetTypeRestriction(value.Expression, value.RuntimeType);
            else
                return BindingRestrictions.GetInstanceRestriction(value.Expression, null);
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

            Expression left, right, mainExpression = null;
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
                }

                if (mainExpression != null)
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

            if (mainExpression == null)
            {
                mainExpression = Expression.Default(ReturnType);
            }

            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
                        .Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.RuntimeType))
                );
        }

        private static Expression ConvertToBigIntegerIfPossible(Expression expression)
        {
            ConstructorInfo constructor;
            if (_bigIntConstructors.TryGetValue(expression.Type, out constructor))
                return Expression.New(constructor, expression);
            else
                return expression;
        }

        private Expression AddOverflowHandler(Expression main, Expression left, Expression right)
        {
            left = ConvertToBigIntegerIfPossible(left);
            right = ConvertToBigIntegerIfPossible(right);

            if (left.Type != right.Type && left.Type != typeof(BigInteger))
                return main;

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
