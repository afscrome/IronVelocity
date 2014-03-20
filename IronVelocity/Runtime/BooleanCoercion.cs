using Microsoft.CSharp.RuntimeBinder;

namespace IronVelocity.Runtime
{
    public static class BooleanCoercion
    {
        public static bool CoerceToBoolean(dynamic value)
        {
            try { return value; }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                return ((object)value) != null;
            }
        }
    }
}
