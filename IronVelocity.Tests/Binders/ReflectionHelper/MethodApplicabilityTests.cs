using IronVelocity.Binders;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Binders
{
    public class MethodApplicabilityTests
    {
        [Test]
        public void ParameterlessMethodIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Parameterless");
            var result = ReflectionHelper.IsMethodApplicable(method);
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithSingleArgumentIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Int");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(int));
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithDerivedArgumentIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Parent");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(Child));
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithIncompatibleSingleArgumentIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Int");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(string));
            Assert.IsFalse(result);
        }

        [Test]
        public void MethodWithParentArgumentIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Child");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(Parent));
            Assert.IsFalse(result);
        }

        [Test]
        public void MethodWithReferenceTypeIsApplicableWithNullValue()
        {
            var method = typeof(TestMethods).GetMethod("Child");
            var result = ReflectionHelper.IsMethodApplicable(method, new Type[] {null});
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithValueTypeIsNotApplicableWithNullValue()
        {
            var method = typeof(TestMethods).GetMethod("Int");
            var result = ReflectionHelper.IsMethodApplicable(method, new Type[] { null });
            Assert.IsFalse(result);
        }

        [Test]
        public void MethodWithGenericSignatureIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Generic");
            var result = ReflectionHelper.IsMethodApplicable(method);
            Assert.IsFalse(result);
        }

        [Test]
        public void MethodWithGenericReturnTypeIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("GenericReturn");
            var result = ReflectionHelper.IsMethodApplicable(method);
            Assert.IsFalse(result);
        }

        [Test]
        public void MethodWithGenericArgumentIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("GenericArg");
            var result = ReflectionHelper.IsMethodApplicable(method, new Type[] { null });
            Assert.IsFalse(result);
        }

        [Test]
        public void ParamsArrayIsApplicableToNoArgument()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = ReflectionHelper.IsMethodApplicable(method);
            Assert.IsTrue(result);
        }

        [Test]
        public void ParamsArrayIsApplicableToSingleArgument()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(string));
            Assert.IsTrue(result);
        }

        [Test]
        public void ParamsArrayIsApplicableToTwoArguments()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(string), typeof(string));
            Assert.IsTrue(result);
        }

        [Test]
        public void StringArrayIsApplicableToStringParamArray()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(string[]));
            Assert.IsTrue(result);
        }

        [Test]
        public void ParamsArrayWithOptionalParamIsNotApplicableToSingleArgument()
        {
            var method = typeof(TestMethods).GetMethod("ParamArrayWithOptional");
            var result = ReflectionHelper.IsMethodApplicable(method, typeof(string));
            Assert.IsFalse(result);
        }


        private class TestMethods
        {
            public void Parameterless() { }
            public void Int(int arg) { }
            public void Parent(Parent arg) { }
            public void Child(Child arg) { }

            public void ParamArray(params string[] args) { }

            public void Generic<T>() { }
            public T GenericReturn<T>() { return default(T); }
            public void GenericArg<T>(T arg) { }

            public void NoMandatoryOneOptional(string s = null) { }
            public void OneMandatoryOneOptional(int i, string s = null) { }
            public void ParamArrayWithOptional(int opt = 0, params string[] args) { }
        }
    }
}
