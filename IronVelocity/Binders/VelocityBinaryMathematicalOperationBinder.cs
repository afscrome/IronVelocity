using IronVelocity.Compilation;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;

namespace IronVelocity.Binders
{
    public class VelocityBinaryMathematicalOperationBinder : BinaryOperationBinder
    {

        public VelocityBinaryMathematicalOperationBinder(ExpressionType type)
            : base(type)
        {
        }

        public override T BindDelegate<T>(System.Runtime.CompilerServices.CallSite<T> site, object[] args)
        {
            return base.BindDelegate<T>(site, args);
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            if (arg == null)
                throw new ArgumentNullException("arg");

            if (!target.HasValue)
                throw new InvalidOperationException();

            //Cannot build a mathematical expression until we've resolved the argument types
            if (!target.HasValue)
                Defer(target);
            if (!arg.HasValue)
                Defer(arg);

            var result = ReturnNullIfEitherArgumentIsNull(target, arg);
            if (result != null)
                return result;

            Expression left, right, mainExpression = null;
            BinaryOperationHelper.MakeArgumentsCompatible(target, arg, out left, out right);

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

        private Expression AddOverflowHandler(Expression main, Expression left, Expression right)
        {
            left = BinaryOperationHelper.ConvertToBigIntegerIfPossible(left);
            right = BinaryOperationHelper.ConvertToBigIntegerIfPossible(right);

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



    }

}
