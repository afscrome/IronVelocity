using System.Numerics;

namespace IronVelocity.Runtime
{
    public static class BigIntegerSimplifier
    {
        public static object ReduceBigInteger(BigInteger value, bool returnSignedIntegers)
        {
            if (returnSignedIntegers)
            {
                if (value >= uint.MinValue && value <= uint.MaxValue)
                    return (uint)value;
                if (value >= ulong.MinValue && value <= ulong.MaxValue)
                    return (ulong)value;
            }
            else
            {
                if (value >= int.MinValue && value <= int.MaxValue)
                    return (int)value;
                if (value >= long.MinValue && value <= long.MaxValue)
                    return (long)value;
            }

            return (float)value;
        }
    }
}
