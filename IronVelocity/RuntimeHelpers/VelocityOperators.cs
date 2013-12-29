﻿using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Diagnostics;

namespace IronVelocity.RuntimeHelpers
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
            try { return left + right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic Subtraction(dynamic left, dynamic right)
        {
            try { return left - right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic Multiplication(dynamic left, dynamic right)
        {
            try { return left * right; }
            catch (RuntimeBinderException) { return null; }
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

        public static dynamic And(dynamic left, dynamic right)
        {
            //TODO: Currently does not support cases where the & operator has been overloaded
            return BooleanCoercion.IsTrue(left) && BooleanCoercion.IsTrue(right);
        }

        public static dynamic Or(dynamic left, dynamic right)
        {
            //TODO: Currently does not support cases where the | operator has been overloaded
            return BooleanCoercion.IsTrue(left) || BooleanCoercion.IsTrue(right);
        }

    }
}
