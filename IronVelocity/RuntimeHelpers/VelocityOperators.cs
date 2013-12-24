using Microsoft.CSharp.RuntimeBinder;

namespace IronVelocity
{
    public static class VelocityOperators
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
        }

        public static dynamic Modulo(dynamic left, dynamic right)
        {
            try { return left % right; }
            catch (RuntimeBinderException) { return null; }
        }

    }
}
