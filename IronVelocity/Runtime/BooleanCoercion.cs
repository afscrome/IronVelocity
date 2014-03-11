
using Microsoft.CSharp.RuntimeBinder;
namespace IronVelocity.Runtime
{
    public static class BooleanCoercion
    {
        /// <summary>
        ///   Coerces the provided object as a boolean and returns True if the value is boolean true.
        /// </summary>
        /// <param name="value">The object to coerce</param>
        /// <returns>True if the object coerced to boolean True, or False if the object coerced to boolean False</returns>
        /// <remarks>
        ///     Note that IsTrue(obj) and IsFalse(obj) are not guaranteed to provide inverse results when the True or False operators have been overridden.
        ///     e.g. bool? with a null value is neither true nor false, hence why we need two separate implementations.
        /// </remarks>
        public static bool IsTrue(dynamic value)
        {
            try { return value; }
            catch (RuntimeBinderException)
            {
                return (object)value != null;
            }
        }

        /// <summary>
        ///   Coerces the provided object as a boolean and returns True if the value is boolean false.
        /// </summary>
        /// <param name="value">The object to coerce</param>
        /// <returns>True if the object coerced to boolean False, or False if the object coerced to boolean True</returns>
        /// <remarks>
        ///     Note that IsTrue(obj) and IsFalse(obj) are not guaranteed to provide inverse results when the True or False operators have been overridden.
        ///     e.g. bool? with a null value is neither true nor false, hence why we need two separate implementations.
        /// </remarks>
        public static bool IsFalse(dynamic value)
        {
            if ((object)value == null)
                return true;

            try { return !value; }
            catch (RuntimeBinderException)
            {
                return (object)value == null;
            }
        }
    }
}
