using Microsoft.CSharp.RuntimeBinder;

namespace IronVelocity.RuntimeHelpers
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

        public static dynamic GreaterThanOrEqual(dynamic left, dynamic right)
        {
            try { return left >= right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic Equal(dynamic left, dynamic right)
        {
            try { return left == right; }
            catch (RuntimeBinderException) { return null; }
        }

        public static dynamic NotEqual(dynamic left, dynamic right)
        {
            try { return left != right; }
            catch (RuntimeBinderException) { return null; }
        }

    }
}
