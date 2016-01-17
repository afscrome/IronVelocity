using IronVelocity.Reflection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace IronVelocity.Tests.Binders.ReflectionHelper
{
    public class MethodResolverTests
    {
        public MethodResolver _resolver = new MethodResolver(new OverloadResolver(new ArgumentConverter()), new ArgumentConverter());

        [Test]
        public void MethodWithGenericSignatureIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("Generic");
            var candidates = GetCandidatesForMethod(method);

            Assert.That(candidates, Has.None.EqualTo(method));
        }

        [Test]
        public void MethodWithGenericReturnTypeIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("GenericReturn");
            var candidates = GetCandidatesForMethod(method);

            Assert.That(candidates, Has.None.EqualTo(method));
        }

        [Test]
        public void MethodWithGenericArgumentIsNotApplicable()
        {
            var method = typeof(TestMethods).GetMethod("GenericArg");
            var candidates = GetCandidatesForMethod(method);

            Assert.That(candidates, Has.None.EqualTo(method));
        }

        private IEnumerable<MethodInfo> GetCandidatesForMethod(MethodBase method)
            => _resolver.GetCandidateMethods(method.DeclaringType.GetTypeInfo(), method.Name);

        private class TestMethods
        {
            public void Generic<T>() { }
            public T GenericReturn<T>() => default(T);
            public void GenericArg<T>(T arg) { }
        }



    }
}
