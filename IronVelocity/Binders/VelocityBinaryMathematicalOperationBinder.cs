using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class VelocityMathematicalOperationBinder : BinaryOperationBinder
    {
        private readonly IArgumentConverter _argumentConverter;
        public MathematicalOperation MathematicalOperation;

        public VelocityMathematicalOperationBinder(MathematicalOperation operation, IArgumentConverter argumentConverter)
            : base(MathematicalOperationToExpressionType(operation))
        {
            _argumentConverter = argumentConverter;
            MathematicalOperation = operation;
        }


        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (arg == null)
                throw new ArgumentNullException(nameof(arg));

            if (!target.HasValue)
                throw new InvalidOperationException();

            //Cannot build a mathematical expression until we've resolved the argument types
            if (!target.HasValue)
                Defer(target);
            if (!arg.HasValue)
                Defer(arg);

            Expression mainExpression = null;
            var restrictions = GetEarlyEscapeRestrictions(target, arg);
            if (restrictions == null)
            {
                var left = VelocityExpressions.ConvertIfNeeded(target);
                var right = VelocityExpressions.ConvertIfNeeded(arg);
                _argumentConverter.MakeBinaryOperandsCompatible(target.RuntimeType, arg.RuntimeType, ref left, ref right);

                if(!TypeHelper.IsNumeric(left.Type))
                {
                    //TODO: Short circuit with Not Numeric BindingRestrictions?
                }
                else if (!TypeHelper.IsNumeric(right.Type))
                {
                    //TODO: Short circuit with Not Numeric BindingRestrictions?
                }
                else
                {
                    mainExpression = GetMathematicalExpression(left, right);

                    if (mainExpression != null)
                    {
                        if (Operation == ExpressionType.Divide || Operation == ExpressionType.Modulo)
                        {
                            if (!TypeHelper.SupportsDivisionByZero(left.Type))
                                mainExpression = AddDivideByZeroHandler(mainExpression, right);
                        }
                        else if (TypeHelper.IsInteger(left.Type) && TypeHelper.IsInteger(right.Type))
                        {
                            mainExpression = AddOverflowHandler(mainExpression, left, right);
                        }
                    }
                }
            }

            if (mainExpression == null)
            {
                BindingEventSource.Log.MathematicalResolutionFailure(Operation, target.LimitType.FullName, arg.LimitType.FullName);
                mainExpression = Expression.Default(ReturnType);
            }
            if (restrictions == null)
            {
                restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType)
                        .Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.RuntimeType));
            }



            return new DynamicMetaObject(
                    VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType),
                    restrictions
                );
        }

        private Expression GetMathematicalExpression(Expression left, Expression right)
        {
            try
            {
                switch (Operation)
                {
                    case ExpressionType.Add:
                        return Expression.AddChecked(left, right);
                    case ExpressionType.Subtract:
                        return Expression.SubtractChecked(left, right);
                    case ExpressionType.Multiply:
                        return Expression.MultiplyChecked(left, right);
                    case ExpressionType.Divide:
                        return Expression.Divide(left, right);
                    case ExpressionType.Modulo:
                        return Expression.Modulo(left, right);
                    default:
                        throw new InvalidProgramException();
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private Expression AddDivideByZeroHandler(Expression mainExpression, Expression right)
        {
            return Expression.Condition(
                Expression.Equal(right, VelocityExpressions.ConvertIfNeeded(Constants.Zero, right.Type)),
                Expression.Default(ReturnType),
                VelocityExpressions.ConvertIfNeeded(mainExpression, ReturnType)
            );
        }

        private Expression AddOverflowHandler(Expression main, Expression left, Expression right)
        {
            left = BigIntegerHelper.ConvertToBigInteger(left);
            right = BigIntegerHelper.ConvertToBigInteger(right);
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

            var useSignedIntegerTypes = TypeHelper.IsSignedInteger(left.Type);
            //Pass the final result into ReduceBigInteger(...) to return a more recognizable primitive
            var overflowFallback = VelocityExpressions.ConvertIfNeeded(
                    Expression.Call(MethodHelpers.ReduceBigIntegerMethodInfo, oveflowHandler, Expression.Constant(useSignedIntegerTypes)),
                    ReturnType                    
            );

            return Expression.TryCatch(
                VelocityExpressions.ConvertIfNeeded(main, ReturnType),
                Expression.Catch(typeof(OverflowException),
                    overflowFallback
                )
            );
        }


        private static BindingRestrictions GetEarlyEscapeRestrictions(DynamicMetaObject left, DynamicMetaObject right)
        {
            //If either value is null, we can't do a mathematical operation
            if (left.Value == null)
                return BindingRestrictions.GetInstanceRestriction(left.Expression, null);
            else if (right.Value == null)
                return BindingRestrictions.GetInstanceRestriction(right.Expression, null);

            // We only support mathematical operations on value types.
            // Using a restriction against Reference Types helps stop us generating a large number of rules for different type combinations
            if (!left.RuntimeType.IsValueType)
                return GetNotValueTypeRestrictions(left.Expression);
            else if (!right.RuntimeType.IsValueType)
                return GetNotValueTypeRestrictions(right.Expression);

            return null;
        }

        private static BindingRestrictions GetNotValueTypeRestrictions(Expression expression)
        {
            var restrictionExpression = Expression.Not(Expression.TypeIs(expression, typeof(ValueType)));
            return BindingRestrictions.GetExpressionRestriction(restrictionExpression);
        }

        private static ExpressionType MathematicalOperationToExpressionType(MathematicalOperation op)
        {
            switch (op)
            {
                case MathematicalOperation.Add:
                    return ExpressionType.Add;
                case MathematicalOperation.Subtract:
                    return ExpressionType.Subtract;
                case MathematicalOperation.Multiply:
                    return ExpressionType.Multiply;
                case MathematicalOperation.Divide:
                    return ExpressionType.Divide;
                case MathematicalOperation.Modulo:
                    return ExpressionType.Modulo;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op));
            }

        }
    }

    public enum MathematicalOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
    }
}
