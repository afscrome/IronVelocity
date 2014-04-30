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
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsInteger(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return type == typeof(BigInteger);
            }
        }

        public static bool SupportsDivisionByZero(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                case TypeCode.Boolean:
                case TypeCode.Empty:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Object:
                    if (type == typeof(BigInteger))
                        return false;
                    else
                        throw new ArgumentOutOfRangeException("type");
                default:
                    return false;
            }
        }
    }
}
