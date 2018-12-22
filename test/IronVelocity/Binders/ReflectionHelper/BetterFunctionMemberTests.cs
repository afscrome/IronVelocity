using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Collections.Immutable;

namespace IronVelocity.Tests.Binders
{
    public class BetterFunctionMemberTests
    {
        private readonly OverloadResolver _overloadResolver = new OverloadResolver(new ArgumentConverter());

        [TestCase(nameof(TestMethods.String), nameof(TestMethods.Object), typeof(string), TestName="ShouldPreferStringToObjectWhenGivenAString")]
        [TestCase(nameof(TestMethods.Int), nameof(TestMethods.Long), typeof(int), TestName = "ShouldPreferIntToLongWhenGivenAnInt")]
        [TestCase(nameof(TestMethods.Int), nameof(TestMethods.UInt), typeof(short), TestName = "ShouldPreferIntToUIntWhenGivenAShort")]
        [TestCase(nameof(TestMethods.UInt), nameof(TestMethods.Long), typeof(ushort), TestName = "ShouldPreferUIntToLongWhenGivenAUShort")]
        [TestCase(nameof(TestMethods.GuidGuid), nameof(TestMethods.ObjectGuid), typeof(Guid), typeof(Guid), TestName = "ShouldPreferGuidGuidToObjectGuidWhenGivenTwoGuids")]
        [TestCase(nameof(TestMethods.GuidGuid), nameof(TestMethods.GuidParams), typeof(Guid), typeof(Guid), TestName = "ShouldPreferGuidGuidToGuidParamsWhenGivenTwoGuids")]
        [TestCase(nameof(TestMethods.IntParams), nameof(TestMethods.LongLong), typeof(int), typeof(int), TestName = "ShouldPreferIntParamsToLongLongWhenGivenTwoInts")]
        [TestCase(nameof(TestMethods.GuidParams), nameof(TestMethods.ObjectParams), typeof(Guid), TestName = "ShouldPreferGuidParamsToObjectParamsWhenGivenGuid")]
        [TestCase(nameof(TestMethods.GuidParams), nameof(TestMethods.ObjectParams), typeof(int[]), TestName = "ShouldPreferIntParamsToLongLongWhenGivenIntArray")]
        [TestCase(nameof(TestMethods.GuidParams), nameof(TestMethods.ObjectParams), typeof(int[]), TestName = "ShouldPreferIntParamsToLongLongWhenGivenIntArray")]
        [TestCase(nameof(TestMethods.Int), nameof(TestMethods.IntParams), typeof(int), TestName = "ShouldPreferIntToIntParamsWhenGivenInt")]
        [TestCase(nameof(TestMethods.IntIntParams), nameof(TestMethods.IntParams), typeof(int), typeof(int), TestName = "ShouldPreferIntIntParamsToIntParamsWhenGivenTwoInts")]
        public void BetterTests(string betterName, string worseName, params Type[] argTypes)
        {
            var result = IsBetterFunctionMember(betterName, worseName, argTypes);
            Assert.AreEqual(BetterResult.Better, result);

            var inverse = IsBetterFunctionMember(worseName, betterName, argTypes);
            Assert.AreEqual(BetterResult.Worse, inverse);
        }

        [TestCase(nameof(TestMethods.ObjectString), nameof(TestMethods.StringObject), TestName = "ShouldBeUnableToDistinguishBetweenStringObjectAndObjectStringWhenGivenStringString")]
        [TestCase(nameof(TestMethods.GuidParams), nameof(TestMethods.ObjectParams), TestName = "ShouldBeUnableToDistinguishBetweenGuidParamsAndObjectParamsWhenGivenNoArguments")]
        public void InseperableTests(string leftName, string rightName)
        {
            var left = nameof(TestMethods.ObjectString);
            var right = nameof(TestMethods.StringObject);
            var args = new[] { typeof(string), typeof(string) };

            var result1 = IsBetterFunctionMember(left, right, args);
            Assert.AreEqual(BetterResult.Incomparable, result1);

            var result2 = IsBetterFunctionMember(right, left, args);
            Assert.AreEqual(BetterResult.Incomparable, result2);
        }

        private BetterResult IsBetterFunctionMember(string leftName, string rightName, params Type[] args)
        {
            var left = typeof(TestMethods).GetMethod(leftName);
            var right = typeof(TestMethods).GetMethod(rightName);


            var leftCandidate = new OverloadResolutionData<MethodBase>(
                leftName.Contains("Params") ? ApplicableForm.Expanded : ApplicableForm.Normal,
                new FunctionMemberData<MethodBase>(left, left.GetParameters())
            );

            var rightCandidate = new OverloadResolutionData<MethodBase>(
                rightName.Contains("Params") ? ApplicableForm.Expanded : ApplicableForm.Normal,
                new FunctionMemberData<MethodBase>(right, right.GetParameters())
            );

            return _overloadResolver.IsBetterFunctionMember(args.ToImmutableList(), leftCandidate, rightCandidate);

        }

        private class TestMethods
        {
            public void Object(object arg){}
            public void String(string arg){}

            public void ObjectGuid(object arg1, Guid arg2) { }
            public void Guid(Guid arg1) { }
            public void GuidGuid(Guid arg1, Guid arg2) { }

            public void ObjectParams(params object[] arg1) { }
            public void GuidParams(params Guid[] arg1) { }
            public void IntParams(params int[] arg1) { }
            public void IntIntParams(int arg1, params int[] arg2) { }

            public void Int(int arg) { }
            public void Long(long arg) { }
            public void LongLong(long arg1, long arg2) { }
            public void UInt(uint arg) { }
            public void UShort(ushort arg) { }

            public void ObjectString(object arg1, string arg2) { }
            public void StringObject(string arg1, object arg2) { }
        }
    }
}
