
namespace IronVelocity
{
    public class VelocityCoercion
    {
        public static bool CoerceObjectToBoolean(object obj)
        {
            return obj is bool
                ? (bool)obj
                : obj != null;

            // Strictly speaking we're missing a case here for a type with custom coercion to bool defined:
            // public static implicit operator bool(MyType) { ... }
            // However this is an edge case we'll ignore for now
        }
    }
}
