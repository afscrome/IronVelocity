using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Diagnostics;
using IronVelocity.Compilation;
using System.Reflection;
using System.Numerics;

namespace IronVelocity.Binders
{
    public class VelocityBinaryMathematicalOperationBinder : VelocityBinaryOperationBinder
    {
        private static IDictionary<Type, ConstructorInfo> _bigIntConstructors = new Dictionary<Type, ConstructorInfo> {
            { typeof(byte), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(int), typeof(BigInteger).GetConstructor(new[] { typeof(int)})},
            { typeof(long), typeof(BigInteger).GetConstructor(new[] { typeof(long)})},
            { typeof(sbyte), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(uint), typeof(BigInteger).GetConstructor(new[] { typeof(uint)})},
            { typeof(ulong), typeof(BigInteger).GetConstructor(new[] { typeof(ulong)})}
        };
        
        public VelocityBinaryMathematicalOperationBinder(ExpressionType type)
            : base(type)
        {
        }


        protected override Expression FallbackBinaryOperationExpression(Expression left, Expression right)
        {
            Expression mainExpression;
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
                    default:
                        throw new InvalidOperationException();
                }
            }
            catch (InvalidOperationException)
            {
                return Expression.Default(this.ReturnType);
            }

            if (Operation == ExpressionType.Divide || Operation == ExpressionType.Modulo)
            {
                throw new NotImplementedException();
            }
            else
            {
                return AddOverflowHandler(mainExpression, left, right);
            }

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
