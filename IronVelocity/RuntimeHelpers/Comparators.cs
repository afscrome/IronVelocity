using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Diagnostics;

namespace IronVelocity.RuntimeHelpers
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="Maintaining Compatability with NVelocity")]
        private static int? Compare(dynamic left, dynamic right)
        {
            if (left == null && right == null)
                return 0;
            if (left == null)
                return -1;
            if (right == null)
                return 1;

            try
            {
                if (left is IComparable)
                    return ((IComparable)left).CompareTo(right);
            }
            catch { }

            try
            {
                if (right is IComparable)
                    return -((IComparable)right).CompareTo(left);
            }
            catch
            {
                /*
                 * I'm not keen on the following, but it is str
                 * Does it make sense to say that ("str" > 0 == true)?
                 * 
                 * Whilst adding this fixes the "CompareString" and "Test Evaluate" regression test from NVelocity,
                 * it breaks the "logical.vm" regression tests from Velocity
                 * 
               */
                if (left is string || right is string)
                    return String.Compare(((object)left).ToString(), ((object)right).ToString(), StringComparison.Ordinal);
  
                Debug.WriteLine("Unable to compare objects '{0}' and '{1}'", ((object)left).GetType(), ((object)right).GetType());
            }

            return null;
        }

    }
}
