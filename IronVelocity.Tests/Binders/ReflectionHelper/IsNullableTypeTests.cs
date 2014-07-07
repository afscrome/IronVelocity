using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Binders
{
    [TestFixture]
    public class IsNullableTypeTests
    {
        [TestCase(typeof(object), true)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(Guid), false)]
        [TestCase(typeof(StringBuilder), true)]
        [TestCase(typeof(int?), true)]
        [TestCase(typeof(Nullable<int>), true)]
        [TestCase(typeof(string), true)]
        public void Test(Type type, bool isNullable)
        {
            Assert.AreEqual(isNullable, ReflectionHelper.IsNullableType(type));
        }
    }
}
