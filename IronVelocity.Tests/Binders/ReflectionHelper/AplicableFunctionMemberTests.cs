using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace IronVelocity.Tests.Binders
{
    public class AplicableFunctionMemberTests
    {
        private readonly OverloadResolver _methodResolver = new OverloadResolver(new ArgumentConverter());

        private bool IsMethodApplicable(MethodInfo method, params Type[] argTypes)
            => _methodResolver.IsApplicableFunctionMember(method.GetParameters(), argTypes.ToImmutableArray());

        [Test]
        public void ParameterlessMethodIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Parameterless");
            var result = IsMethodApplicable(method);
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithSingleArgumentIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Int");
            var result = IsMethodApplicable(method, typeof(int));
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithDerivedArgumentIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Parent");
            var result = IsMethodApplicable(method, typeof(Child));
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithIncompatibleSingleArgumentIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Int");
            var result = IsMethodApplicable(method, typeof(string));
            Assert.IsFalse(result);
        }

        [Test]
        public void MethodWithParentArgumentIsApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Child");
            var result = IsMethodApplicable(method, typeof(Parent));
            Assert.IsFalse(result);
        }

        [Test]
        public void MethodWithReferenceTypeIsApplicableWithNullValue()
        {
            var method = typeof(TestMethods).GetMethod("Child");
            var result = IsMethodApplicable(method, new Type[] {null});
            Assert.IsTrue(result);
        }

        [Test]
        public void MethodWithValueTypeIsNotApplicableWithNullValue()
        {
            var method = typeof(TestMethods).GetMethod("Int");
            var result = IsMethodApplicable(method, new Type[] { null });
            Assert.IsFalse(result);
        }


        [Test]
        public void ParamsArrayIsApplicableToNoArgument()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = IsMethodApplicable(method);
            Assert.IsTrue(result);
        }

        [Test]
        public void ParamsArrayIsApplicableToSingleArgument()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = IsMethodApplicable(method, typeof(string));
            Assert.IsTrue(result);
        }

        [Test]
        public void ParamsArrayIsApplicableToTwoArguments()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = IsMethodApplicable(method, typeof(string), typeof(string));
            Assert.IsTrue(result);
        }

        [Test]
        public void StringArrayIsApplicableToStringParamArray()
        {
            var method = typeof(TestMethods).GetMethod("ParamArray");
            var result = IsMethodApplicable(method, typeof(string[]));
            Assert.IsTrue(result);
        }

        [Test]
        public void ParamsArrayWithOptionalParamIsNotApplicableToSingleArgument()
        {
            var method = typeof(TestMethods).GetMethod("ParamArrayWithOptional");
            var result = IsMethodApplicable(method, typeof(string));
            Assert.IsFalse(result);
        }


        private class TestMethods
        {
            public void Parameterless() { }
            public void Int(int arg) { }
            public void Parent(Parent arg) { }
            public void Child(Child arg) { }

            public void ParamArray(params string[] args) { }

            public void NoMandatoryOneOptional(string s = null) { }
            public void OneMandatoryOneOptional(int i, string s = null) { }
            public void ParamArrayWithOptional(int opt = 0, params string[] args) { }
        }
    }
}
