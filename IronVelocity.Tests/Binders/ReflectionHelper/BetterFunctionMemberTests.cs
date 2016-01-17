using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Reflection;

namespace IronVelocity.Tests.Binders
{
    public class BetterFunctionMemberTests
    {
        private readonly OverloadResolver _overoladResolver = new OverloadResolver(new ArgumentConverter());

        [TestCase("String", "Object", TestName="String_BetterThan_Object")]
        [TestCase("Int", "Long", TestName = "Int_BetterThan_Long")]
        [TestCase("GuidGuid", "ObjectGuid", TestName = "Guid_BetterThan_Object_With_CommonArgumentType")]
        [TestCase("GuidGuid", "GuidParams", TestName = "Guid_BetterThan_GuidParams")]
        [TestCase("GuidGuid", "GuidParams", TestName = "GuidGuid_BetterThan_GuidParams")]
        public void BetterTests(string betterName, string worseName)
        {
            var left = typeof(TestMethods).GetMethod(betterName);
            var right = typeof(TestMethods).GetMethod(worseName);

            var result = IsBetterFunctionMember(left, right);
            Assert.AreEqual(FunctionMemberComparisonResult.Better, result);

            var inverse = IsBetterFunctionMember(right, left);
            Assert.AreEqual(FunctionMemberComparisonResult.Worse, inverse);
        }

        [TestCase("Inseperable1", "Inseperable2")]
        public void InseperableTests(string leftName, string rightName)
        {
            var left = typeof(TestMethods).GetMethod(leftName);
            var right = typeof(TestMethods).GetMethod(rightName);

            var result1 = IsBetterFunctionMember(left, right);
            Assert.AreEqual(FunctionMemberComparisonResult.Incomparable, result1);

            var result2 = IsBetterFunctionMember(right, left);
            Assert.AreEqual(FunctionMemberComparisonResult.Incomparable, result2);
        }

        private FunctionMemberComparisonResult IsBetterFunctionMember(MethodBase left, MethodBase right)
            => _overoladResolver.IsBetterFunctionMember(left?.GetParameters(), right?.GetParameters());

        private class TestMethods
        {
            public void Object(object arg){}
            public void String(string arg){}

            public void ObjectGuid(object arg1, Guid arg2) { }
            public void GuidGuid(Guid arg1, Guid arg2) { }

            public void GuidParams(params Guid[] arg1) { }

            public void Int(int arg) { }
            public void Long(long arg) { }

            public void Inseperable1(object arg1, string arg2) { }
            public void Inseperable2(string arg1, object arg2) { }
        }
    }
}
