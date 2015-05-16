using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IronVelocity.Tests.Binders
{
    /// <summary>
    /// This class performs tests against ReflectionHelper.IsArgumentCompatible
    /// </summary>
    public class ImplicitConversionSupportTests
    {
        private readonly IArgumentConverter _conversionHelper = new ArgumentConverter();
        //TODO: 6.1.11?? - User-defined implicit conversions

        //6.1.1 - Identity Conversions
        [TestCase(typeof(sbyte), TestName = "Identity Conversion: Primitive")]
        [TestCase(typeof(Child), TestName = "Identity Conversion: Custom")]
        [TestCase(typeof(EnvironmentVariableTarget), TestName = "Identity Conversion: Enum")]
        [TestCase(typeof(string[]), TestName = "Identity Conversion: Array")]
        [TestCase(typeof(Guid), TestName = "Identity Conversion: Struct")]
        public void IdentityConversion(Type type)
        {
            ImplicitConversionTest(type, type, true);
        }

        //6.1.2 - Implicit numeric conversions - At end of file.

        //6.1.3 - Enumeration conversions
        //WRONG - not how the C# spec states.  However should we work this way in Velocity since
        //VTL doesn't have a concept of enums?  Better to support conversion of string to enum?
        /*
        [TestCase(typeof(EnvironmentVariableTarget), typeof(int), true)]
        [TestCase(typeof(int), typeof(EnvironmentVariableTarget), false)]
        [TestCase(typeof(EnvironmentVariableTarget), typeof(long), true)]
        [TestCase(typeof(long), typeof(EnvironmentVariableTarget), false)]
        public void EnumerationConversion(Type from, Type to, bool expected)
        {
            var result = _conversionHelper.CanBeConverted(from, to);
            Assert.AreEqual(expected, result);
        }
        */

        //6.1.4 Implicit nullable conversions
        [TestCase(typeof(int?), typeof(long?), true, Ignore = true)]
        [TestCase(typeof(long?), typeof(int?), false)]
        [TestCase(typeof(int), typeof(long?), true, Ignore = true)]
        [TestCase(typeof(long), typeof(int?), false)]
        public void ImplicitNullableConversions(Type from, Type to, bool isConvertable)
        {
            ImplicitConversionTest(from, to, isConvertable);
        }

        //6.1.5 Null literal conversions
        [TestCase(typeof(Child), true)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(Guid), false)]
        public void NullLiteralConversion(Type type, bool isConvertable)
        {
            ImplicitConversionTest(null, type, isConvertable);
        }

        //6.1.6 Implicit reference conversions
        //Reference to object
        [TestCase(typeof(Child), typeof(object), true)]
        // Class S to Class T
        [TestCase(typeof(object), typeof(Child), false)]
        [TestCase(typeof(Child), typeof(Parent), true)]
        [TestCase(typeof(Parent), typeof(Child), false)]
        // Class S to Interface T
        [TestCase(typeof(List<string>), typeof(IList), true)]
        [TestCase(typeof(ICollection), typeof(List<string>), false)]
        //Interface S to Interface T
        [TestCase(typeof(IList<string>), typeof(ICollection<string>), true)]
        [TestCase(typeof(ICollection<string>), typeof(IList<string>), false)]
        [TestCase(typeof(IReadOnlyList<string>), typeof(IReadOnlyCollection<object>), true)]
        [TestCase(typeof(IReadOnlyCollection<string>), typeof(IReadOnlyList<object>), false)]
        //S[] to T[]
        [TestCase(typeof(string[]), typeof(object[]), true)]
        [TestCase(typeof(int[]), typeof(object[]), false)]
        [TestCase(typeof(object[]), typeof(Child[]), false)]
        [TestCase(typeof(Child[,]), typeof(object[,]), true)]
        [TestCase(typeof(object[,]), typeof(string[,]), false)]
        [TestCase(typeof(string[]), typeof(string[,]), false)]
        [TestCase(typeof(int[]), typeof(long[]), false)]
        [TestCase(typeof(int[]), typeof(object[]), false)]
        // S[] to Array
        [TestCase(typeof(string[]), typeof(Array), true)]
        [TestCase(typeof(string[]), typeof(IStructuralComparable), true)]
        // S[] to IList<T>
        [TestCase(typeof(string[]), typeof(IList<string>), true)]
        [TestCase(typeof(string[]), typeof(ICollection), true)]
        [TestCase(typeof(string[,]), typeof(IList<string>), false)]
        public void ImplicitReferenceConversions(Type from, Type to, bool isConvertable)
        {
            ImplicitConversionTest(from, to, isConvertable);
        }

        //6.1.7 Boxing Conversions
        [Test]
        public void BoxingConversion()
        {
            ImplicitConversionTest(typeof(int), typeof(object), true);
        }

        //6.1.8 Implicit dynamic conversions -N/A
        //6.1.9 Implicit constant expression conversions - N/A

        //6.1.10 Implicit conversions involving type paramaters
        [TestCase(typeof(IReadOnlyList<IReadOnlyList<string>>), typeof(IReadOnlyList<IReadOnlyCollection<string>>), true)]
        [TestCase(typeof(IReadOnlyList<IReadOnlyCollection<string>>), typeof(IReadOnlyList<IReadOnlyList<string>>), false)]
        [TestCase(typeof(IReadOnlyList<List<string>>), typeof(IReadOnlyList<IReadOnlyCollection<string>>), true)]
        [TestCase(typeof(IReadOnlyList<IReadOnlyCollection<string>>), typeof(List<IReadOnlyList<string>>), false)]
        public void ImplicitConversionInvolvingTypeParameters(Type from, Type to, bool isConvertable)
        {
            ImplicitConversionTest(from, to, isConvertable);
        }

        //6.1.2 - Implicit Numeric conversions
        //SByte
        [TestCase(typeof(sbyte),   typeof(byte),    false, TestName = "Primitive Implicit Conversion: sbyte to byte")]
        [TestCase(typeof(sbyte),   typeof(short),   true,  TestName = "Primitive Implicit Conversion: sbyte to short")]
        [TestCase(typeof(sbyte),   typeof(ushort),  false, TestName = "Primitive Implicit Conversion: sbyte to ushort")]
        [TestCase(typeof(sbyte),   typeof(int),     true,  TestName = "Primitive Implicit Conversion: sbyte to int")]
        [TestCase(typeof(sbyte),   typeof(uint),    false, TestName = "Primitive Implicit Conversion: sbyte to uint")]
        [TestCase(typeof(sbyte),   typeof(long),    true,  TestName = "Primitive Implicit Conversion: sbyte to long")]
        [TestCase(typeof(sbyte),   typeof(ulong),   false, TestName = "Primitive Implicit Conversion: sbyte to ulong")]
        [TestCase(typeof(sbyte),   typeof(char),    false, TestName = "Primitive Implicit Conversion: sbyte to char")]
        [TestCase(typeof(sbyte),   typeof(float),   true,  TestName = "Primitive Implicit Conversion: sbyte to float")]
        [TestCase(typeof(sbyte),   typeof(double),  true,  TestName = "Primitive Implicit Conversion: sbyte to double")]
        [TestCase(typeof(sbyte),   typeof(decimal), true,  TestName = "Primitive Implicit Conversion: sbyte to decimal")]
        //Byte
        [TestCase(typeof(byte),    typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: byte to sbyte")]
        [TestCase(typeof(byte),    typeof(short),   true,  TestName = "Primitive Implicit Conversion: byte to short")]
        [TestCase(typeof(byte),    typeof(ushort),  true,  TestName = "Primitive Implicit Conversion: byte to ushort")]
        [TestCase(typeof(byte),    typeof(int),     true,  TestName = "Primitive Implicit Conversion: byte to int")]
        [TestCase(typeof(byte),    typeof(uint),    true,  TestName = "Primitive Implicit Conversion: byte to uint")]
        [TestCase(typeof(byte),    typeof(long),    true,  TestName = "Primitive Implicit Conversion: byte to long")]
        [TestCase(typeof(byte),    typeof(ulong),   true,  TestName = "Primitive Implicit Conversion: byte to ulong")]
        [TestCase(typeof(byte),    typeof(char),    false, TestName = "Primitive Implicit Conversion: byte to char")]
        [TestCase(typeof(byte),    typeof(float),   true,  TestName = "Primitive Implicit Conversion: byte to float")]
        [TestCase(typeof(byte),    typeof(double),  true,  TestName = "Primitive Implicit Conversion: byte to double")]
        [TestCase(typeof(byte),    typeof(decimal), true,  TestName = "Primitive Implicit Conversion: byte to decimal")]
        //Int16
        [TestCase(typeof(short),   typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: short to sbyte")]
        [TestCase(typeof(short),   typeof(byte),    false, TestName = "Primitive Implicit Conversion: short to byte")]
        [TestCase(typeof(short),   typeof(ushort),  false, TestName = "Primitive Implicit Conversion: short to ushort")]
        [TestCase(typeof(short),   typeof(int),     true,  TestName = "Primitive Implicit Conversion: short to int")]
        [TestCase(typeof(short),   typeof(uint),    false, TestName = "Primitive Implicit Conversion: short to uint")]
        [TestCase(typeof(short),   typeof(long),    true,  TestName = "Primitive Implicit Conversion: short to long")]
        [TestCase(typeof(short),   typeof(ulong),   false, TestName = "Primitive Implicit Conversion: short to ulong")]
        [TestCase(typeof(short),   typeof(char),    false, TestName = "Primitive Implicit Conversion: short to char")]
        [TestCase(typeof(short),   typeof(float),   true,  TestName = "Primitive Implicit Conversion: short to float")]
        [TestCase(typeof(short),   typeof(double),  true,  TestName = "Primitive Implicit Conversion: short to double")]
        [TestCase(typeof(short),   typeof(decimal), true,  TestName = "Primitive Implicit Conversion: short to decimal")]
        //UInt16       
        [TestCase(typeof(ushort),  typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: ushort to sbyte")]
        [TestCase(typeof(ushort),  typeof(byte),    false, TestName = "Primitive Implicit Conversion: ushort to byte")]
        [TestCase(typeof(ushort),  typeof(short),   false, TestName = "Primitive Implicit Conversion: ushort to short")]
        [TestCase(typeof(ushort),  typeof(int),     true,  TestName = "Primitive Implicit Conversion: ushort to int")]
        [TestCase(typeof(ushort),  typeof(uint),    true,  TestName = "Primitive Implicit Conversion: ushort to uint")]
        [TestCase(typeof(ushort),  typeof(long),    true,  TestName = "Primitive Implicit Conversion: ushort to long")]
        [TestCase(typeof(ushort),  typeof(ulong),   true,  TestName = "Primitive Implicit Conversion: ushort to ulong")]
        [TestCase(typeof(ushort),  typeof(char),    false, TestName = "Primitive Implicit Conversion: ushort to char")]
        [TestCase(typeof(ushort),  typeof(float),   true,  TestName = "Primitive Implicit Conversion: ushort to float")]
        [TestCase(typeof(ushort),  typeof(double),  true,  TestName = "Primitive Implicit Conversion: ushort to double")]
        [TestCase(typeof(ushort),  typeof(decimal), true,  TestName = "Primitive Implicit Conversion: ushort to decimal")]
        //Int32   
        [TestCase(typeof(int),     typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: int to sbyte")]
        [TestCase(typeof(int),     typeof(byte),    false, TestName = "Primitive Implicit Conversion: int to byte")]
        [TestCase(typeof(int),     typeof(short),   false, TestName = "Primitive Implicit Conversion: int to short")]
        [TestCase(typeof(int),     typeof(ushort),  false, TestName = "Primitive Implicit Conversion: int to ushort")]
        [TestCase(typeof(int),     typeof(uint),    false, TestName = "Primitive Implicit Conversion: int to uint")]
        [TestCase(typeof(int),     typeof(long),    true,  TestName = "Primitive Implicit Conversion: int to long")]
        [TestCase(typeof(int),     typeof(ulong),   false, TestName = "Primitive Implicit Conversion: int to ulong")]
        [TestCase(typeof(int),     typeof(char),    false, TestName = "Primitive Implicit Conversion: int to char")]
        [TestCase(typeof(int),     typeof(float),   true,  TestName = "Primitive Implicit Conversion: int to float")]
        [TestCase(typeof(int),     typeof(double),  true,  TestName = "Primitive Implicit Conversion: int to double")]
        [TestCase(typeof(int),     typeof(decimal), true,  TestName = "Primitive Implicit Conversion: int to decimal")]
        //UInt32        
        [TestCase(typeof(uint),    typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: uint to sbyte")]
        [TestCase(typeof(uint),    typeof(byte),    false, TestName = "Primitive Implicit Conversion: uint to byte")]
        [TestCase(typeof(uint),    typeof(short),   false, TestName = "Primitive Implicit Conversion: uint to short")]
        [TestCase(typeof(uint),    typeof(ushort),  false, TestName = "Primitive Implicit Conversion: uint to ushort")]
        [TestCase(typeof(uint),    typeof(int),     false, TestName = "Primitive Implicit Conversion: uint to int")]
        [TestCase(typeof(uint),    typeof(long),    true,  TestName = "Primitive Implicit Conversion: uint to long")]
        [TestCase(typeof(uint),    typeof(ulong),   true,  TestName = "Primitive Implicit Conversion: uint to ulong")]
        [TestCase(typeof(uint),    typeof(char),    false, TestName = "Primitive Implicit Conversion: uint to char")]
        [TestCase(typeof(uint),    typeof(float),   true,  TestName = "Primitive Implicit Conversion: uint to float")]
        [TestCase(typeof(uint),    typeof(double),  true,  TestName = "Primitive Implicit Conversion: uint to double")]
        [TestCase(typeof(uint),    typeof(decimal), true,  TestName = "Primitive Implicit Conversion: uint to decimal")]
        //Int64               
        [TestCase(typeof(long),    typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: long to sbyte")]
        [TestCase(typeof(long),    typeof(byte),    false, TestName = "Primitive Implicit Conversion: long to byte")]
        [TestCase(typeof(long),    typeof(short),   false, TestName = "Primitive Implicit Conversion: long to short")]
        [TestCase(typeof(long),    typeof(ushort),  false, TestName = "Primitive Implicit Conversion: long to ushort")]
        [TestCase(typeof(long),    typeof(int),     false, TestName = "Primitive Implicit Conversion: long to int")]
        [TestCase(typeof(long),    typeof(uint),    false, TestName = "Primitive Implicit Conversion: long to uint")]
        [TestCase(typeof(long),    typeof(ulong),   false, TestName = "Primitive Implicit Conversion: long to ulong")]
        [TestCase(typeof(long),    typeof(char),    false, TestName = "Primitive Implicit Conversion: long to char")]
        [TestCase(typeof(long),    typeof(float),   true,  TestName = "Primitive Implicit Conversion: long to float")]
        [TestCase(typeof(long),    typeof(double),  true,  TestName = "Primitive Implicit Conversion: long to double")]
        [TestCase(typeof(long),    typeof(decimal), true,  TestName = "Primitive Implicit Conversion: long to decimal")]
        //UInt64
        [TestCase(typeof(ulong),   typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: ulong to sbyte")]
        [TestCase(typeof(ulong),   typeof(byte),    false, TestName = "Primitive Implicit Conversion: ulong to byte")]
        [TestCase(typeof(ulong),   typeof(short),   false, TestName = "Primitive Implicit Conversion: ulong to short")]
        [TestCase(typeof(ulong),   typeof(ushort),  false, TestName = "Primitive Implicit Conversion: ulong to ushort")]
        [TestCase(typeof(ulong),   typeof(int),     false, TestName = "Primitive Implicit Conversion: ulong to int")]
        [TestCase(typeof(ulong),   typeof(uint),    false, TestName = "Primitive Implicit Conversion: ulong to uint")]
        [TestCase(typeof(ulong),   typeof(long),    false, TestName = "Primitive Implicit Conversion: ulong to long")]
        [TestCase(typeof(ulong),   typeof(char),    false, TestName = "Primitive Implicit Conversion: ulong to char")]
        [TestCase(typeof(ulong),   typeof(float),   true,  TestName = "Primitive Implicit Conversion: ulong to float")]
        [TestCase(typeof(ulong),   typeof(double),  true,  TestName = "Primitive Implicit Conversion: ulong to double")]
        [TestCase(typeof(ulong),   typeof(decimal), true,  TestName = "Primitive Implicit Conversion: ulong to decimal")]
        //Char
        [TestCase(typeof(char),    typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: char to sbyte")]
        [TestCase(typeof(char),    typeof(byte),    false, TestName = "Primitive Implicit Conversion: char to byte")]
        [TestCase(typeof(char),    typeof(short),   false, TestName = "Primitive Implicit Conversion: char to short")]
        [TestCase(typeof(char),    typeof(ushort),  true,  TestName = "Primitive Implicit Conversion: char to ushort")]
        [TestCase(typeof(char),    typeof(int),     true,  TestName = "Primitive Implicit Conversion: char to int")]
        [TestCase(typeof(char),    typeof(uint),    true,  TestName = "Primitive Implicit Conversion: char to uint")]
        [TestCase(typeof(char),    typeof(long),    true,  TestName = "Primitive Implicit Conversion: char to long")]
        [TestCase(typeof(char),    typeof(ulong),   true,  TestName = "Primitive Implicit Conversion: char to ulong")]
        [TestCase(typeof(char),    typeof(float),   true,  TestName = "Primitive Implicit Conversion: char to float")]
        [TestCase(typeof(char),    typeof(double),  true,  TestName = "Primitive Implicit Conversion: char to double")]
        [TestCase(typeof(char),    typeof(decimal), true,  TestName = "Primitive Implicit Conversion: char to decimal")]
        //Single precision floating point
        [TestCase(typeof(float),   typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: float to sbyte")]
        [TestCase(typeof(float),   typeof(byte),    false, TestName = "Primitive Implicit Conversion: float to byte")]
        [TestCase(typeof(float),   typeof(short),   false, TestName = "Primitive Implicit Conversion: float to short")]
        [TestCase(typeof(float),   typeof(ushort),  false, TestName = "Primitive Implicit Conversion: float to ushort")]
        [TestCase(typeof(float),   typeof(int),     false, TestName = "Primitive Implicit Conversion: float to int")]
        [TestCase(typeof(float),   typeof(uint),    false, TestName = "Primitive Implicit Conversion: float to uint")]
        [TestCase(typeof(float),   typeof(long),    false, TestName = "Primitive Implicit Conversion: float to long")]
        [TestCase(typeof(float),   typeof(ulong),   false, TestName = "Primitive Implicit Conversion: float to ulong")]
        [TestCase(typeof(float),   typeof(char),    false, TestName = "Primitive Implicit Conversion: float to char")]
        [TestCase(typeof(float),   typeof(double),  true,  TestName = "Primitive Implicit Conversion: float to double")]
        [TestCase(typeof(float),   typeof(decimal), false, TestName = "Primitive Implicit Conversion: float to decimal")]
        //Double precision floating point
        [TestCase(typeof(double),  typeof(sbyte),   false, TestName = "Primitive Implicit Conversion: double to sbyte")]
        [TestCase(typeof(double),  typeof(byte),    false, TestName = "Primitive Implicit Conversion: double to byte")]
        [TestCase(typeof(double),  typeof(short),   false, TestName = "Primitive Implicit Conversion: double to short")]
        [TestCase(typeof(double),  typeof(ushort),  false, TestName = "Primitive Implicit Conversion: double to ushort")]
        [TestCase(typeof(double),  typeof(int),     false, TestName = "Primitive Implicit Conversion: double to int")]
        [TestCase(typeof(double),  typeof(uint),    false, TestName = "Primitive Implicit Conversion: double to uint")]
        [TestCase(typeof(double),  typeof(long),    false, TestName = "Primitive Implicit Conversion: double to long")]
        [TestCase(typeof(double),  typeof(ulong),   false, TestName = "Primitive Implicit Conversion: double to ulong")]
        [TestCase(typeof(double),  typeof(char),    false, TestName = "Primitive Implicit Conversion: double to char")]
        [TestCase(typeof(double),  typeof(float),   false, TestName = "Primitive Implicit Conversion: double to float")]
        [TestCase(typeof(double),  typeof(decimal), false, TestName = "Primitive Implicit Conversion: double to decimal")]
        public void NumericConversionTests(Type from, Type to, bool isConvertable)
        {
            ImplicitConversionTest(from, to, isConvertable);
        }


        private void ImplicitConversionTest(Type from, Type to, bool isConvertable)
        {
            var implicitConversionResult = _conversionHelper.GetConverter(from, to) is ImplicitExpressionConverter;

            if (isConvertable)
                Assert.That(implicitConversionResult, Is.True);
            else
                Assert.That(implicitConversionResult, Is.False);
        }
    }
}
