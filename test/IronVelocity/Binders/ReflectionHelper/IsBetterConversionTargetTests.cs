using IronVelocity.Reflection;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Binders
{
    public class IsBetterConversionTargetTests
    {
        private readonly OverloadResolver _overloadResolver = new OverloadResolver(new ArgumentConverter());

        [TestCase(typeof(string), typeof(object), true)]
        [TestCase(typeof(int), typeof(long), true)]
        [TestCase(typeof(int), typeof(uint), true)]
        [TestCase(typeof(uint), typeof(int), false)]
        // Implicit conversion from T2 to T1 exists
        [TestCase(typeof(object), typeof(string), false)]
        [TestCase(typeof(long), typeof(int), false)]
        [TestCase(typeof(long), typeof(uint), false)]
        // T1 is a signed integral type and T2 is an unsigned integral type
        [TestCase(typeof(sbyte), typeof(byte), true)]
        [TestCase(typeof(sbyte), typeof(ushort), true)]
        [TestCase(typeof(sbyte), typeof(uint), true)]
        [TestCase(typeof(sbyte), typeof(ulong), true)]
        [TestCase(typeof(short), typeof(ushort), true)]
        [TestCase(typeof(short), typeof(uint), true)]
        [TestCase(typeof(short), typeof(ulong), true)]
        [TestCase(typeof(int), typeof(uint), true)]
        [TestCase(typeof(int), typeof(ulong), true)]
        [TestCase(typeof(long), typeof(ulong), true)]
        public void ConversionTargetTest(Type candidate, Type other, bool expectedResult)
        {
            var result = _overloadResolver.IsBetterConversionTarget(candidate, other);

            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
