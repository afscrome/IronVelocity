using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Diagnostics;

namespace IronVelocity.Runtime
{
    public static class Comparators
    {
        private static bool? UndefinedComparison = false;

        public static bool? LessThan(dynamic left, dynamic right)
        {
            try { return left < right; }
            catch (RuntimeBinderException)
            {
                int? comparison = Compare(left, right);
                if (comparison.HasValue)
                    return comparison < 0;
                else
                    return UndefinedComparison;
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
                    return UndefinedComparison;
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
                    return UndefinedComparison;
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
                    return UndefinedComparison;
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
        private static int? Compare(object left, object right)
        {
            if (left == right)
                return 0;
            if (left == null)
                return -1;
            if (right == null)
                return 1;

            var leftComparable = left as IComparable;
            if (leftComparable != null)
            {
                try { return leftComparable.CompareTo(right); }
                catch { }
            }

            var rightComparable = right as IComparable;
            if (rightComparable != null)
            {
                try { return -rightComparable.CompareTo(left); }
                catch { }
            }

            /*
             * I'm not keen on the following, but it is what NVelocity did so leaving it for backwards compatability
             * Does it make sense to say that ("str" > 0 == true)?
             * 
             * Whilst adding this fixes the "CompareString" and "Test Evaluate" regression test from NVelocity,
             * it breaks the "logical.vm" regression tests from Velocity
             * 
             * 
             * TODO: Per discussion with Ben, do the ToString for string / primitives
             */

            //If either is a char, convert to a string to simplify conversions:

            var leftType = left.GetType();
            var rightType = right.GetType();

            if (leftType == typeof(string) || rightType == typeof(string))
            {
                //Try converting to numbers 
                // * comparing strings with numbers
                // * comparing numeric types which don't have compatible comparision operators (e.g. UInt16 and long)
                double leftNum, rightNum;
                if (Double.TryParse(left.ToString(), out leftNum) && Double.TryParse(right.ToString(), out rightNum))
                    return leftNum.CompareTo(rightNum);
            }
            if (leftType.IsEnum || rightType.IsEnum)
            {
                return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
            }
            else if ((leftType == typeof(string) || leftType == typeof(char)) && (rightType == typeof(string) || rightType == typeof(char)))
                return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);

            Debug.WriteLine("Unable to compare objects '{0}' and '{1}'", left.GetType(), right.GetType());
            return null;
        }

    }
}
