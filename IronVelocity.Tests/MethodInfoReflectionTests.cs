using IronVelocity.Compilation;
using NUnit.Framework;

namespace IronVelocity.Tests
{
    public class MethodInfoReflectionTests
    {
        [Test]
        public void Append()
        {
            Assert.NotNull(MethodHelpers.AppendMethodInfo);
        }

        [Test]
        public void ToString()
        {
            Assert.NotNull(MethodHelpers.ToStringMethodInfo);
        }

        [Test]
        public void List()
        {
            Assert.NotNull(MethodHelpers.ListConstructorInfo);
        }

        [Test]
        public void IntegerRange()
        {
            Assert.NotNull(MethodHelpers.IntegerRangeMethodInfo);
        }

        [Test]
        public void StringConcat()
        {
            Assert.NotNull(MethodHelpers.StringConcatMethodInfo);
        }
        [Test]
        public void BooleanCoercion()
        {
            Assert.NotNull(MethodHelpers.BooleanCoercionMethodInfo);
        }
    }
}
