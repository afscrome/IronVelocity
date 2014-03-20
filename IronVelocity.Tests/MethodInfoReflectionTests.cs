using IronVelocity.Compilation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void Addition()
        {
            Assert.NotNull(MethodHelpers.AdditionMethodInfo);
        }

        [Test]
        public void Subtraction()
        {
            Assert.NotNull(MethodHelpers.SubtractionMethodInfo);
        }

        [Test]
        public void Multiplication()
        {
            Assert.NotNull(MethodHelpers.MultiplicationMethodInfo);
        }

        [Test]
        public void Division()
        {
            Assert.NotNull(MethodHelpers.DivisionMethodInfo);
        }

        [Test]
        public void Modulo()
        {
            Assert.NotNull(MethodHelpers.ModuloMethodInfo);
        }

        [Test]
        public void LessThan()
        {
            Assert.NotNull(MethodHelpers.LessThanMethodInfo);
        }

        [Test]
        public void LessThanOrEqual()
        {
            Assert.NotNull(MethodHelpers.LessThanOrEqualMethodInfo);
        }

        [Test]
        public void GreaterThan()
        {
            Assert.NotNull(MethodHelpers.GreaterThanMethodInfo);
        }

        [Test]
        public void GreaterThanOrEqualTo()
        {
            Assert.NotNull(MethodHelpers.GreaterThanOrEqualMethodInfo);
        }

        [Test]
        public void Equal()
        {
            Assert.NotNull(MethodHelpers.EqualMethodInfo);
        }

        [Test]
        public void NotEqual()
        {
            Assert.NotNull(MethodHelpers.NotEqualMethodInfo);
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
