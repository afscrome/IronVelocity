using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Diagnostics;

namespace IronVelocity.Runtime
{
    public static class Comparators
    {
        public static bool? LessThan(dynamic left, dynamic right)
        {
            try { return left < right; }
            catch (RuntimeBinderException)
            {
                int? comparison = Compare(left, right);
                if (comparison.HasValue)
                    return comparison < 0;
                else
                    return null;
            }
        }

        public static bool? LessThanOrEqual(dynamic left, dynamic right)
        {
            try { return left <= right; }
            catch (RuntimeBinderException)
            {
                int? comparison = Compare(left, right);
                if (comparison.HasValue)
                    return comparison <= 0;
                else
                    return null;
            }
        }

        public static bool? GreaterThan(dynamic left, dynamic right)
        {
            try { return left > right; }
            catch (RuntimeBinderException)
            {
                int? comparison = Compare(left, right);
                if (comparison.HasValue)
                    return comparison > 0;
                else
                    return null;
            }
        }

        public static bool? GreaterThanOrEqual(dynamic left, dynamic right)
        {
            try { return left >= right; }
            catch (RuntimeBinderException)
            {
                int? comparison = Compare(left, right);
                if (comparison.HasValue)
                    return comparison >= 0;
                else
                    return null;
            }
        }

        public static bool Equal(dynamic left, dynamic right)
        {
            try { return left == right; }
            catch (RuntimeBinderException)
            {
                int? comparison = Compare(left, right);
                if (comparison.HasValue)
                    return comparison == 0;
                else
                    return Object.Equals(left, right);
            }
        }

        public static bool NotEqual(dynamic left, dynamic right)
        {
            try { return left != right; }
            catch (RuntimeBinderException)
            {
                int? comparison = Compare(left, right);
                if (comparison.HasValue)
                    return comparison != 0;
                else
                    return !Object.Equals(left, right);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Maintaining Compatability with NVelocity")]
        private static int? Compare(dynamic left, dynamic right)
        {
            if (left == null && right == null)
                return 0;
            if (left == null)
                return -1;
            if (right == null)
                return 1;
            
            if (left is IComparable)
            {
                try { return ((IComparable)left).CompareTo(right); }
                catch { }
            }

            if (right is IComparable)
            {
                try { return -((IComparable)right).CompareTo(left); }
                catch { }
            }

            /*
             * I'm not keen on the following, but it is what NVelocity did so leaving it for backwards compatability
             * Does it make sense to say that ("str" > 0 == true)?
             * 
             * Whilst adding this fixes the "CompareString" and "Test Evaluate" regression test from NVelocity,
             * it breaks the "logical.vm" regression tests from Velocity
             * 
           */

            //Casting to object so we use static invocation for ToString and GetType.
            var leftObj = (object)left;
            var rightObj = (object)right;

            if (left is string || right is string)
            {
                return String.Compare(leftObj.ToString(), rightObj.ToString(), StringComparison.Ordinal);
            }
            else
            {
                Debug.WriteLine("Unable to compare objects '{0}' and '{1}'", leftObj.GetType(), rightObj.GetType());
                return null;
            }
        }

    }
}
