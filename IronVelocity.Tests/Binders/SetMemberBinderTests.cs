using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity.Tests.Binders
{
    public class SetMemberBinderTests : BinderTestBase
    {

        [Test]
        public void ClassSetPropertyNameIsExactMatch()
        {
            var input = new BasicClass();
            TestAssignmentOnReferenceType(input, "Property", "abc123");
            Assert.AreEqual("abc123", input.Property);
        }

        [Test]
        public void ClassSetPropertyNameDiffersInCase()
        {
            var input = new BasicClass();
            TestAssignmentOnReferenceType(input, "pRoPeRtY", "other");
            Assert.AreEqual("other", input.Property);
        }

        [Test]
        public void ClassSetPropertyWithInvalidType()
        {
            var input = new BasicClass();
            TestAssignmentOnReferenceType(input, "Property", true);
            Assert.AreEqual("Success!", input.Property);
        }

        [Test]
        public void ClassSetValueTypeProperty()
        {
            var input = new BasicClass();
            var result = Guid.NewGuid();
            TestAssignmentOnReferenceType(input, "ValueType", result);
            Assert.AreEqual(result, input.ValueType);
        }

        [Test]
        public void ClassSetPrimitiveProperty()
        {
            var input = new BasicClass();
            TestAssignmentOnReferenceType(input, "Primitive", 123);
            Assert.AreEqual(123, input.Primitive);
        }

        [Test]
        public void ShouldSetPropetyWithSetterOnly()
        {
            var input = new FunkyProperties();
            TestAssignmentOnReferenceType(input, nameof(input.SetterOnly), 789);

            Assert.That(input.GetSetterOnlyValue(), Is.EqualTo(789));
        }

        [Test]
        public void ShouldSetPropetyWithPublicSetterPrivateGetter()
        {
            var input = new FunkyProperties();
            TestAssignmentOnReferenceType(input, nameof(input.PrivateGetPublicSet), 'f');

            Assert.That(input.GetPrivateGetterValue(), Is.EqualTo('f'));
        }

        [Test]
        public void ShouldIgnorePropetyWithPublicGetPrivateSetter()
        {
            var input = new FunkyProperties();
            TestAssignmentOnReferenceType(input, nameof(input.PublicGetPrivateSetter), "new");

            Assert.That(input.PublicGetPrivateSetter, Is.EqualTo("original"));
        }

        [Test]
        public void ShouldIgnorePropertyWithGetterOnly()
        {
            var input = new FunkyProperties();
            TestAssignmentOnReferenceType(input, nameof(input.GetterOnly), 7.12);

            Assert.That(input.GetterOnly, Is.EqualTo(3.142));
        }

        [Test]
        public void ShouldIgnoreSetOnMethodCall()
        {
            var input = new FunkyProperties();
            TestAssignmentOnReferenceType(input, nameof(input.GetSetterOnlyValue), 78);

            Assert.That(input.GetSetterOnlyValue, Is.EqualTo(123));
        }

        private void TestAssignmentOnReferenceType<TTarget, TValue>(TTarget input, string memberName, TValue value)
            where TTarget : class
        {
            var binder = new VelocitySetMemberBinder(memberName, new MemberResolver());

            InvokeBinder(binder, input, value);
        }


        #region Struct
        [Test]
        public void StructSetPropertyNameIsExactMatch()
        {
            var input = new BasicStruct("initial");
            TestAssignmentOnStruct(ref input, "Property", "abc123");
            Assert.AreEqual("abc123", input.Property);
        }

        [Test]
        public void StructSetPropertyNameDiffersInCase()
        {
            var input = new BasicStruct("initial");
            TestAssignmentOnStruct(ref input, "pRoPeRtY", "other");
            Assert.AreEqual("other", input.Property);
        }

        [Test]
        public void StructSetPropertyWithInvalidType()
        {
            var input = new BasicStruct("Success!");
            TestAssignmentOnStruct(ref input, "Property", true);
            Assert.AreEqual("Success!", input.Property);
        }

        [Test]
        public void StructSetValueTypeProperty()
        {
            var input = new BasicStruct("initial");
            var result = Guid.NewGuid();
            TestAssignmentOnStruct(ref input, "ValueType", result);
            Assert.AreEqual(result, input.ValueType);
        }

        [Test]
        public void StructSetPrimitiveProperty()
        {
            var input = new BasicStruct("initial");
            TestAssignmentOnStruct(ref input, "Primitive", 123);
            Assert.AreEqual(123, input.Primitive);
        }
        #endregion


        private delegate void StructByReferenceExecution<T>(ref T input) where T: struct;
        private delegate void StructByReferenceDynamicDelegate<TTarget, TValue>(CallSite callSite, ref TTarget target, TValue value) where TTarget : struct;

        private void TestAssignmentOnStruct<TTarget, TValue>(ref TTarget input, string memberName, TValue value)
            where TTarget : struct
        {
            // Remember that structs are copied when passed into another method.  If we're not careful about how we can construct this test
            // we can end up with a scenario in which the tests can never pass because we're modifying a copy of the struct, not the original
            // meaning our assertions fail because the modifications were made on a copy
            // Be VERY CAREFUL modifying this method to ensure you don't accidently introduce such a regression.
            
            // To avoid this, we need to do two things
            // 1. Rather than quoting the value, pass it as a ByRef parameter into a dynamic method
            // 2. When creating the Dynamic Expression using the binder,we need to use Expression.MakeDynamic() rather than Expression.Dynamic()
            //    The delegate created by Expression.Dynamic() doesn't support ByRef parameters, hence why we have to make the delegate ourselves.
            //    See https://dlr.codeplex.com/discussions/69200 for more details.

            
            var binder = new VelocitySetMemberBinder(memberName, new MemberResolver());

            var methodParameter = Expression.Parameter(typeof(TTarget).MakeByRefType());
            var delegateType = typeof(StructByReferenceDynamicDelegate<TTarget, TValue>);
            var setExpression = Expression.MakeDynamic(delegateType, binder, methodParameter, Expression.Constant(value));
            //var setExpression = Expression.Dynamic(binder, typeof(void), structByRefParam, Expression.Constant(value));

            var lambda = Expression.Lambda<StructByReferenceExecution<TTarget>>(setExpression, false, new[] {methodParameter});
            var method = lambda.Compile();
            method(ref input);
        }

        public class BasicClass
        {
            public string Field = "Success!";
            private string _property = "Success!";
            public string Property
            {
                get { return _property; }
                set { _property = value; }
            }

            public Guid ValueType { get; set; }
            public int Primitive { get; set; }
        }

        public struct BasicStruct
        {
            public BasicStruct(string value)
            {
                Field = value;
                _property = value;
                _valueType = Guid.Empty;
                _primitive = 0;
            }

            public string Field;
            private string _property;
            public string Property
            {
                get { return _property; }
                set { _property = value; }
            }

            private int _primitive;
            public int Primitive
            {
                get { return _primitive; }
                set { _primitive = value; }
            }

            private Guid _valueType;
            public Guid ValueType
            {
                get { return _valueType; }
                set { _valueType = value; }
            }
        }

        public struct ConstantMembers
        {
            private const string _privateConstant = "Hi Everybody";
            public const string PublicConstant = "Hi Doctor Nick";
        }

        public class PrivateMembers
        {
            private int _privateField = 5678;
            private int _privateProperty => _privateField;
        }

        public struct StaticMembers
        {
            public static Guid StaticField = Guid.NewGuid();
            public static Guid StaticProperty = StaticField;
        }

        public class AmbigiousNames
        {
            public string POTENTIALLY_AMBIGIOUS_PROPERTY { get; set; }
            public string potentially_ambigious_property { get; set; }

            public string POTENTIALLY_AMBIGIOUS_FIELD;
            public string potentially_ambigious_field;
        }

        public class FunkyProperties
        {
            private int _setterOnly = 123;
            public string PublicGetPrivateSetter { get; private set; } = "original";
            public char PrivateGetPublicSet { private get; set; } = 'c';

            public int SetterOnly { set { _setterOnly= value; } }
            public double GetterOnly { get; } = 3.142;

            public int GetSetterOnlyValue() => _setterOnly;
            public char GetPrivateGetterValue() => PrivateGetPublicSet;
        }
    }
}
