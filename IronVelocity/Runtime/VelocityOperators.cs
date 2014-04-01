using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Diagnostics;
using System.Numerics;

namespace IronVelocity.Runtime
{
    public static class Operators
    {
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
    }
}
