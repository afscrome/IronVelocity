
namespace IronVelocity.Runtime
{
    public static class BooleanCoercion
    {
        public static bool CoerceToBoolean(object value)
        {
            if (value is bool)
                return (bool)value;
            else
                return ((object)value) != null;
        }
    }
}
