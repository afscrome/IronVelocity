using System;
using System.Numerics;

namespace IronVelocity
{
    public static class TypeHelper
    {
        public static bool IsNumeric(Type type)
        {
            return IsInteger(type) || IsNonIntegerNumeric(type);
        }

        public static bool IsNonIntegerNumeric(Type type)
        {
            return type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);
        }

        public static bool IsSignedInteger(Type type)
        {
            return type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(sbyte);
        }
        public static bool IsUnsignedInteger(Type type)
        {
            return type == typeof(byte)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(ushort);
        }

        public static bool IsInteger(Type type) => IsSignedInteger(type) || IsUnsignedInteger(type);

        public static bool SupportsDivisionByZero(Type type) => type == typeof(Single) || type == typeof(BigInteger);

        public static bool IsNullableType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return !(type.IsValueType && Nullable.GetUnderlyingType(type) == null);
        }
    }
}
