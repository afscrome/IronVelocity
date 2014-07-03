using IronVelocity.Compilation;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

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
        public new void ToString()
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
        public void ReduceBigInteger()
        {
            Assert.NotNull(MethodHelpers.ReduceBigIntegerMethodInfo);
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

        [Test]
        public void SetAsyncMethodBuilderStateMachine()
        {
            Assert.NotNull(MethodHelpers.SetAsyncMethodBuilderStateMachine);
        }

        [Test]
        public void DebuggableAttributeConstructorInfo()
        {
            Assert.NotNull(MethodHelpers.DebuggableAttributeConstructorInfo);
        }

        [Test]
        public void DebuggerHiddenConstructorInfo()
        {
            Assert.NotNull(MethodHelpers.DebuggerHiddenConstructorInfo);
        }
    }
}
