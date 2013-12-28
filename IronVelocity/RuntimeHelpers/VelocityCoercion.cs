
using Microsoft.CSharp.RuntimeBinder;
namespace IronVelocity
{
    public static class VelocityCoercion
    {
        public static bool CoerceObjectToBoolean(dynamic obj)
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
            catch(RuntimeBinderException)
            {
                return obj != null;
            }
        }
    }
}
