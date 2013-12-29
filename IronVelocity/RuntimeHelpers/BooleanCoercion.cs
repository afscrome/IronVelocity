
using Microsoft.CSharp.RuntimeBinder;
namespace IronVelocity.RuntimeHelpers
{
    public static class BooleanCoercion
    {
        /// <summary>
        ///   Coerces the provided object as a boolean and returns True if the value is boolean true.
        /// </summary>
        /// <param name="obj">The object to coerce</param>
        /// <returns>True if the object coerced to boolean True, or False if the object coerced to boolean False</returns>
        /// <remarks>
        ///     Note that IsTrue(obj) and IsFalse(obj) are not guaranteed to provide inverse results when the True or False operators have been overridden.
        ///     hence why we need two separate implementations.
        /// </remarks>
        public static bool IsTrue(dynamic obj)
        {
            if (obj is bool)
                return (bool)obj;

            try
            {
                //Special case if the object has overridden the true or false operator
                if (obj)
                    return true;
                else
                    return false;
            }
            catch (RuntimeBinderException)
            {
                return obj != null;
            }
        }

        /// <summary>
        ///   Coerces the provided object as a boolean and returns True if the value is boolean false.
        /// </summary>
        /// <param name="obj">The object to coerce</param>
        /// <returns>True if the object coerced to boolean False, or False if the object coerced to boolean True</returns>
        /// <remarks>
        ///     Note that IsTrue(obj) and IsFalse(obj) are not guaranteed to provide inverse results when the True or False operators have been overridden.
        ///     hence why we need two separate implementations.
        /// </remarks>
        public static bool IsFalse(dynamic obj)
        {
            if (obj is bool)
                return !((bool)obj);

            try
            {
                //Special case if the object has overridden the false operator
                if (!obj)
                    return true;
                else
                    return false;
            }
            catch (RuntimeBinderException)
            {
                return obj == null;
            }
        }
    }
}
