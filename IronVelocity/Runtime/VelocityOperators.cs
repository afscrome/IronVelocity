using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Diagnostics;
using System.Numerics;

namespace IronVelocity.Runtime
{
    public static class Operators
    {

        public static dynamic Addition(dynamic left, dynamic right)
        {
            /*Removing the following makes the code 50% faster when neither value is null, but 20% slower when one value is null
             * Considering one side being null should be an edge case, optimise for non null
                    if (left == null || right == null)
                        return null;
            */
            try { checked { return left + right; } }
            catch (RuntimeBinderException) { return null; }
            catch (OverflowException)
            {
                try
                {
                    var bigLeft = new BigInteger(left);
                    var bigRight = new BigInteger(right);

                    return CompatibleBigIntegerType(bigLeft + bigRight);
                }
                catch (RuntimeBinderException) { return null; }
            }
        }

        public static dynamic Subtraction(dynamic left, dynamic right)
        {
            try { checked { return left - right; } }
            catch (RuntimeBinderException) { return null; }
            catch (OverflowException)
            {
                try
                {
                    var bigLeft = new BigInteger(left);
                    var bigRight = new BigInteger(right);

                    return CompatibleBigIntegerType(bigLeft - bigRight);
                }
                catch (RuntimeBinderException) { return null; }
            }
        }

        public static dynamic Multiplication(dynamic left, dynamic right)
        {
            try { checked { return left * right; } }
            catch (RuntimeBinderException) { return null; }
            catch (OverflowException)
            {
                try
                {
                    var bigLeft = new BigInteger(left);
                    var bigRight = new BigInteger(right);

                    return CompatibleBigIntegerType(bigLeft * bigRight);
                }
                catch (RuntimeBinderException) { return null; }
            }
        }

        public static dynamic Division(dynamic left, dynamic right)
        {
            try { return left / right; }
            catch (RuntimeBinderException) { return null; }
            catch (DivideByZeroException)
            {
                Debug.WriteLine("Attempt to divide by zero");
                return null;
            }
        }

        public static dynamic Modulo(dynamic left, dynamic right)
        {
            try { return left % right; }
            catch (RuntimeBinderException) { return null; }
            catch (DivideByZeroException)
            {
                Debug.WriteLine("Attempt to divide by zero");
                return null;
            }
        }

        public static dynamic And(object left, object right)
        {
            return BooleanCoercion.IsTrue(left) && BooleanCoercion.IsTrue(right);
            // Since it's virtually impossible to override the And operator, no need for dynamics
            /*
            try { return left && right; }
            catch (RuntimeBinderException)
            {
                return BooleanCoercion.IsTrue(left) && BooleanCoercion.IsTrue(right);
            }
            */
        }

        public static dynamic Or(object left, object right)
        {
            return BooleanCoercion.IsTrue(left) || BooleanCoercion.IsTrue(right);
            // Since it's virtually impossible to override the Or operator, no need for dynamics
            /*
            try { return left || right; }
            catch (RuntimeBinderException)
            {
                return BooleanCoercion.IsTrue(left) || BooleanCoercion.IsTrue(right);
            }
             */
        }


        private static object CompatibleBigIntegerType(BigInteger value)
        {
            if (value <= int.MinValue && value >= int.MaxValue)
                return (int)value;
            if (value <= long.MinValue && value >= long.MaxValue)
                return (long)value;
            if (value <= ulong.MinValue && value >= ulong.MaxValue)
                return (ulong)value;

            return (float)value;
        }
    }
}
