﻿using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Binders
{
    public class GetMemberBinderTests : BinderTestBase
    {

        #region Properties

        [Test]
        public void ClassPropertyNameIsExactMatch()
        {
            var input = new BasicClass();
            var result = Get(input, "Property");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassPropertyNameDiffersInCase()
        {
            var input = new BasicClass();
            var result = Get(input, "pRoPeRtY");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassPropertyDoesNotExist()
        {
            var input = new BasicClass();
            var result = Get(input, "BlahBlahBlah");
            Assert.Null(result);
        }

        [Test]
        public void StructPropertyNameIsExactMatch()
        {
            var input = new BasicStruct("Yeah!");
            var result = Get(input, "Property");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructPropertyNameDiffersInCase()
        {
            var input = new BasicStruct("Yeah!");
            var result = Get(input, "pRoPeRtY");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructPropertyDoesNotExist()
        {
            var input = new BasicStruct("Yeah!");
            var result = Get(input, "BlahBlahBlah");
            Assert.Null(result);
        }

        [Test]
        public void AmbigiousPropertyNames_NoExactMatch()
        {
            var input = new AmbigiousNames
            {
                POTENTIALLY_AMBIGIOUS_PROPERTY = "foo",
                potentially_ambigious_property = "bar",
            };

            var result = Get(input, "potentially_AMBIGIOUS_property");
            Assert.Null(result);
        }

        [Test]
        public void AmbigiousProperties_UpperCaseProperty()
        {
            var input = new AmbigiousNames
            {
                POTENTIALLY_AMBIGIOUS_PROPERTY = "foo",
                potentially_ambigious_property = "bar",
            };

            var result = Get(input, "POTENTIALLY_AMBIGIOUS_PROPERTY");
            Assert.AreEqual("foo", result);
        }

        [Test]
        public void AmbigiousProperties_LowerCaseProperty()
        {
            var input = new AmbigiousNames
            {
                POTENTIALLY_AMBIGIOUS_PROPERTY = "burger",
                potentially_ambigious_property = "chips",
            };

            var result = Get(input, "potentially_ambigious_property");
            Assert.AreEqual("chips", result);
        }

        [Test]
        public void PrivateProperty()
        {
            var input = new PrivateMembers();

            var result = Get(input, "_privateProperty");
            Assert.Null(result);
        }

        [Test]
        public void StaticProperty()
        {
            var input = new StaticMembers();

            var result = Get(input, "StaticProperty");
            Assert.Null(result);
        }

        [Test]
        public void ExplictInterfaceProperty()
        {
            var input = new BasicClass();

            var result = Get(input, "Hidden");
            Assert.Null(result);
        }

        [Test]
        public void ExplictInterfacePropertyWithConflicts()
        {
            var input = new BasicClass();
            var result = Get(input, "HiddenConflict");

            Assert.IsNull(result);
        }
        #endregion

        #region Fields

        [Test]
        public void ClassFieldNameIsExactMatch()
        {
            var input = new BasicClass();
            var result = Get(input, "Field");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassFieldNameDiffersInCase()
        {
            var input = new BasicClass();
            var result = Get(input, "Field");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassFieldDoesNotExist()
        {
            var input = new BasicClass();
            var result = Get(input, "BlahBlahBlah");
            Assert.Null(result);
        }

        [Test]
        public void StructFieldNameIsExactMatch()
        {
            var input = new BasicStruct("Yeah!");
            var result = Get(input, "Field");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructFieldNameDiffersInCase()
        {
            var input = new BasicStruct("Yeah!");
            var result = Get(input, "Field");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructFieldDoesNotExist()
        {
            var input = new BasicStruct("Yeah!");
            var result = Get(input, "BlahBlahBlah");
            Assert.Null(result);
        }

        [Test]
        public void AmbigiousFieldNames_NoExactMatch()
        {
            var input = new AmbigiousNames
            {
                POTENTIALLY_AMBIGIOUS_FIELD = "foo",
                potentially_ambigious_field = "bar",
            };

            var result = Get(input, "potentially_AMBIGIOUS_field");
            Assert.Null(result);
        }

        [Test]
        public void AmbigiousProperties_UpperCaseField()
        {
            var input = new AmbigiousNames
            {
                POTENTIALLY_AMBIGIOUS_FIELD = "foo",
                potentially_ambigious_field = "bar",
            };

            var result = Get(input, "POTENTIALLY_AMBIGIOUS_FIELD");
            Assert.AreEqual("foo", result);
        }

        [Test]
        public void AmbigiousProperties_LowerCaseField()
        {
            var input = new AmbigiousNames
            {
                POTENTIALLY_AMBIGIOUS_FIELD = "burger",
                potentially_ambigious_field = "chips",
            };

            var result = Get(input, "potentially_ambigious_field");
            Assert.AreEqual("chips", result);
        }

        [Test]
        public void PrivateField()
        {
            var input = new PrivateMembers();

            var result = Get(input, "_privateField");
            Assert.Null(result);
        }
        [Test]
        public void StaticField()
        {
            var input = new StaticMembers();

            var result = Get(input, "StaticField");
            Assert.Null(result);
        }

        #endregion

        #region Methods
        [Test]
        [Ignore("NVelocity specific")]
        public void PublicMethodWithZeroArgumentsIsInvokedAsMember()
        {
            var input = new MethodsWith0Parameters();

            var result = Get(input, "DoSomething");
            Assert.AreEqual("Hello World", result);
        }

        [Test]
        public void PrivateMethodWithZeroArgumentsIsNotInvokedAsMember()
        {
            var input = new MethodsWith0Parameters();

            var result = Get(input, "Secret");
            Assert.IsNull(result);
        }

        [Test]
        public void StaticMethodWithZeroArgumentsIsNotInvokedAsMember()
        {
            var input = new MethodsWith0Parameters();

            var result = Get(input, "Static");
            Assert.IsNull(result);
        }

        #endregion


        [Test]
        public void NullInput()
        {
            object input = null;
            var result = Get(input, "Field");
            Assert.Null(result);
        }
        
        [Test]
        [Ignore("NVelocity specific")]
        public void ShouldReturnEnumValueWhenMemberOnEnumType()
        {
            var input = typeof(UriFormat);
            var result = Get(input, "Unescaped");

            Assert.That(result, Is.EqualTo(UriFormat.Unescaped));
        }

        [Test]
        public void ShouldGetPropetyWithGetterOnly()
        {
            var input = new FunkyProperties();
            var result = Get(input, nameof(input.GetterOnly));

            Assert.That(result, Is.EqualTo(3.142));
        }

        [Test]
        public void ShouldGetPropetyWithPublicGetPrivateSet()
        {
            var input = new FunkyProperties();
            var result = Get(input, nameof(input.PublicGetPrivateSetter));

            Assert.That(result, Is.EqualTo("original"));
        }

        [Test]
        public void ShouldIgnorePropetyWithPrivateGetPublicSet()
        {
            var input = new FunkyProperties();
            var result = Get(input, nameof(input.PrivateGetPublicSet));

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ShouldIgnorePropertyWithSetterOnly()
        {
            var input = new FunkyProperties();
            var result = Get(input, nameof(input.SetterOnly));

            Assert.That(result, Is.Null);
        }


        private object Get(object input, string memberName)
        {
            var binder = new VelocityGetMemberBinder(memberName, new MemberResolver());
            return InvokeBinder(binder, input);
        }

        public class BasicClass : IExplicit, IConflict
        {
            public string Field = "Success!";
            public string Property => Field;


            string IExplicit.Hidden => "Super Secret";
            string IExplicit.HiddenConflict => "Conflict";
            string IConflict.HiddenConflict => "Conflict";
        }


        public struct BasicStruct
        {
            public BasicStruct(string value)
            {
                Field = value;
            }

            public string Field;
            public string Property => Field;
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
            public static Guid StaticProperty => StaticField;
        }

        public class AmbigiousNames
        {
            public string POTENTIALLY_AMBIGIOUS_PROPERTY { get; set; }
            public string potentially_ambigious_property { get; set; }

            public string POTENTIALLY_AMBIGIOUS_FIELD;
            public string potentially_ambigious_field;
        }

        public interface IExplicit
        {
            string Hidden { get; }
            string HiddenConflict { get; }
        }

        public interface IConflict
        {
            string HiddenConflict { get; }
        }

        public class MethodsWith0Parameters
        {
            public string DoSomething() => "Hello World";
            public static string Static() => "Fail";
            private string Secret() => "Fail";
        }

        public class FunkyProperties
        {
            private int _setterOnly = 123;
            public string PublicGetPrivateSetter { get; private set; } = "original";
            public char PrivateGetPublicSet { private get; set; } = 'c';

            public int SetterOnly { set { _setterOnly = value; } }
            public double GetterOnly { get; } = 3.142;
        }

    }
}
