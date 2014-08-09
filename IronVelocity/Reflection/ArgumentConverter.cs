using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Reflection
{
    public class ArgumentConverter : IArgumentConverter
    {
        /// <summary>
        /// A list of implicit numeric conversions as defined in section 6.1.2 of the C# 5.0 spec
        /// The value array indicates types which the Key type can be implicitly converted to.
        /// </summary>
        private static readonly IDictionary<Type, Type[]> _implicitNumericConversions = new Dictionary<Type, Type[]>(){
                    { typeof(sbyte), new[]{typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(byte), new[]{typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(short), new[]{typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(ushort), new[]{typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(int), new[]{typeof(long), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(uint), new[]{typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(long), new[]{typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(ulong), new[]{typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(char), new[]{typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)}},
                    { typeof(float), new[]{typeof(double)}},
                };

        public bool CanBeConverted(Type from, Type to)
        {
            //from may be null, but to may not be
            if (to == null)
                throw new ArgumentNullException("to");


            if (from == null)
                return !to.IsValueType;

            if (to.IsAssignableFrom(from))
                return true;

            if (!from.IsPrimitive)
                return false;

            switch (Type.GetTypeCode(from))
            {
                case TypeCode.Byte:
                    break;
                case TypeCode.Char:
                    break;
                case TypeCode.Decimal:
                    break;
                case TypeCode.Double:
                    break;
                case TypeCode.Int16:
                    break;
                case TypeCode.Int32:
                    break;
                case TypeCode.Int64:
                    break;
                case TypeCode.SByte:
                    break;
                case TypeCode.Single:
                    break;
                case TypeCode.UInt16:
                    break;
                case TypeCode.UInt32:
                    break;
                case TypeCode.UInt64:
                    break;
                default:
                    break;
            }

            Type[] supportedConversions;
            if (_implicitNumericConversions.TryGetValue(from, out supportedConversions))
                return supportedConversions.Contains(to);

            return false;
        }

    }
}
