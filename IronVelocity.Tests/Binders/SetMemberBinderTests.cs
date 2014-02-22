using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Tests.Binders
{
    public class SetMemberBinderTests
    {

        [Test]
        public void ClassSetPropertyNameIsExactMatch()
        {
            var input = new BasicClass();
            test(input, "Property", "abc123");
            Assert.AreEqual("abc123", input.Property);
        }

        [Test]
        public void ClassSetPropertyNameDiffersInCase()
        {
            var input = new BasicClass();
            test(input, "pRoPeRtY", "other");
            Assert.AreEqual("other", input.Property);
        }

        [Test]
        public void ClassSetPropertyDoesNotExist()
        {
            var input = new BasicClass();
            test(input, "NonExistant", "other");
            //How to test?
            Assert.Inconclusive();
        }

        [Test]
        public void ClassSetPropertyWithInvalidType()
        {
            var input = new BasicClass();
            test(input, "Property", true);
            Assert.AreEqual("Success!", input.Property);
        }

        [Test]
        public void ClassSetValueTypeProperty()
        {
            var input = new BasicClass();
            test(input, "ValueType", 123);
            Assert.AreEqual(123, input.ValueType);
        }

        #region Struct
        [Test]
        [Ignore("TODO: Setting to a struct works in 'BoxTestWithPropertySet' test, so why not here?")]
        public void StructSetPropertyNameIsExactMatch()
        {
            var input = new BasicStruct("initial");
            test(input, "Property", "abc123");
            Assert.AreEqual("abc123", input.Property);
        }

        [Test]
        [Ignore("TODO: Setting to a struct works in 'BoxTestWithPropertySet' test, so why not here?")]
        public void StructSetPropertyNameDiffersInCase()
        {
            var input = new BasicStruct("initial");
            test(input, "pRoPeRtY", "other");
            Assert.AreEqual("other", input.Property);
        }

        [Test]
        [Ignore("TODO: Setting to a struct works in 'BoxTestWithPropertySet' test, so why not here?")]
        public void StructSetPropertyDoesNotExist()
        {
            var input = new BasicStruct("initial");
            test(input, "NonExistant", "other");
            //How to test?
            Assert.Inconclusive();
        }

        [Test]
        public void StructSetPropertyWithInvalidType()
        {
            var input = new BasicStruct("Success!");
            test(input, "Property", true);
            Assert.AreEqual("Success!", input.Property);
        }

        [Test]
        [Ignore("TODO: Setting to a struct works in 'BoxTestWithPropertySet' test, so why not here?")]
        public void StructSetValueTypeProperty()
        {
            var input = new BasicStruct("initial");
            test(input, "ValueType", 123);
            Assert.AreEqual(123, input.ValueType);
        }
        #endregion

        private void test<TInput,TValue>(TInput input, string memberName, TValue value)
        {
            var binder = new VelocitySetMemberBinder(memberName);

            var inputObject = Expression.Constant(input);
            var inputValue = Expression.Constant(value);
            var expression = Expression.Dynamic(binder, typeof(object), inputObject, inputValue);

            var lambda = Expression.Lambda<Func<object>>(expression);

            var action = lambda.Compile();

            action();
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

            public int ValueType { get; set; }
        }

        public struct BasicStruct
        {
            public BasicStruct(string value)
            {
                Field = value;
                _property = value;
                _valueType = 0;
            }

            public string Field;
            private string _property;
            public string Property
            {
                get { return _property; }
                set { _property = value; }
            }

            private int _valueType;
            public int ValueType
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
            private int _privateProperty { get { return _privateField; } }
        }

        public struct StaticMembers
        {
            public static Guid StaticField = Guid.NewGuid();
            public static Guid StaticProperty { get { return StaticField; } }
        }

        public class AmbigiousNames
        {
            public string POTENTIALLY_AMBIGIOUS_PROPERTY { get; set; }
            public string potentially_ambigious_property { get; set; }

            public string POTENTIALLY_AMBIGIOUS_FIELD;
            public string potentially_ambigious_field;
        }
    }
}
