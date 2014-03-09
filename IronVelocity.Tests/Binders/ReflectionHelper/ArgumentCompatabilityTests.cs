using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Reflection;

namespace IronVelocity.Tests.Binders
{
    //Tests based on section 6.1 from C# spec
    public class ArgumentCompatabilityTests
    {
        [Test]
        public void ChildTypeIsCompatibleWithParent()
        {
            var param = GetParameterInfo("Parent");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(Child), param);
            Assert.IsTrue(result);
        }
        [Test]
        public void ParentTypeIsIncompatibleWithChild()
        {
            var param = GetParameterInfo("Child");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(Parent), param);
            Assert.IsFalse(result);
        }

        [Test]
        public void SiblingTypesAreIncompatible()
        {
            var param = GetParameterInfo("Son");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(Daughter), param);
            Assert.IsFalse(result);
        }
        
        [Test]
        public void UnderlyingTypeIsCompatibleWithParamsArray()
        {
            var param = GetParameterInfo("ParamArray");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(Child), param);
            Assert.IsTrue(result);
        }

        [Test]
        public void ParentOfUnderlyingTypeIsIncompatibleWithParamsArray()
        {
            var param = GetParameterInfo("ParamArray");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(Parent), param);
            Assert.IsFalse(result);
        }

        [Test]
        public void ChildOfUnderlyingTypeIsIncompatibleWithParamsArray()
        {
            var param = GetParameterInfo("ParamArray");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(Son), param);
            Assert.IsTrue(result);
        }

        [Test]
        public void ValueTypeCanBeBoxedToObject()
        {
            var param = GetParameterInfo("Object");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(int), param);
            Assert.IsTrue(result);
        }

        [Test]
        public void GenericTypeArgumentNotSupported()
        {
            var param = GetParameterInfo("GenericArgument");
            var result = ReflectionHelper.IsArgumentCompatible(typeof(object), param);
            Assert.IsFalse(result);
        }

        private ParameterInfo GetParameterInfo(string key)
        {
            return typeof(TestArguments).GetMethod(key).GetParameters()[0];
        }

        /// <summary>
        /// Fake class used by GetParameterInfo to build a ParamaterInfo object
        /// </summary>
        private class TestArguments
        {
            public void ValueType(int arg) { }
            public void ReferenceType(String arg) { }
            public void BoxedPrimitive(Int32 arg) { }
            public void Parent(Parent arg) { }
            public void Child(Child arg) { }
            public void Son(Son arg) { }
            public void ParamArray(params Child[] arg) { }
            public void Object(object arg) { }
            public void GenericArgument<T>(T arg) { }

            public void NullableValueType(int? arg) { }
        }

    }
}
