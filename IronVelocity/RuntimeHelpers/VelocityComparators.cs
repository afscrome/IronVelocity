using Microsoft.CSharp.RuntimeBinder;

namespace IronVelocity
{
    public static class Comparators
    {


        public static dynamic LessThan(dynamic left, dynamic right)
        {
            try { return left < right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic LessThanOrEqual(dynamic left, dynamic right)
        {
            try { return left <= right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic GreaterThan(dynamic left, dynamic right)
        {
            try { return left > right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic GreaterThanOrEqualTo(dynamic left, dynamic right)
        {
            try { return left >= right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic EqualTo(dynamic left, dynamic right)
        {
            try { return left == right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic NotEqualTo(dynamic left, dynamic right)
        {
            try { return left != right; }
            catch (RuntimeBinderException) { return null; }
        }

    }
}
