using IronVelocity.Binders;
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
            var result = test(input, "Property");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassPropertyNameDiffersInCase()
        {
            var input = new BasicClass();
            var result = test(input, "pRoPeRtY");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassPropertyDoesNotExist()
        {
            var input = new BasicClass();
            var result = test(input, "BlahBlahBlah");
            Assert.Null(result);
        }

        [Test]
        public void StructPropertyNameIsExactMatch()
        {
            var input = new BasicStruct("Yeah!");
            var result = test(input, "Property");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructPropertyNameDiffersInCase()
        {
            var input = new BasicStruct("Yeah!");
            var result = test(input, "pRoPeRtY");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructPropertyDoesNotExist()
        {
            var input = new BasicStruct("Yeah!");
            var result = test(input, "BlahBlahBlah");
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

            var result = test(input, "potentially_AMBIGIOUS_property");
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

            var result = test(input, "POTENTIALLY_AMBIGIOUS_PROPERTY");
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

            var result = test(input, "potentially_ambigious_property");
            Assert.AreEqual("chips", result);
        }

        [Test]
        public void PrivateProperty()
        {
            var input = new PrivateMembers();

            var result = test(input, "_privateProperty");
            Assert.Null(result);
        }

        [Test]
        public void StaticProperty()
        {
            var input = new StaticMembers();

            var result = test(input, "StaticProperty");
            Assert.Null(result);
        }

        [Test]
        public void ExplictInterfaceProperty()
        {
            var input = new BasicClass();

            var result = test(input, "Hidden");
            Assert.Null(result);
        }

        [Test]
        public void ExplictInterfacePropertyWithConflicts()
        {
            var input = new BasicClass();
            var result = test(input, "HiddenConflict");

            Assert.IsNull(result);
        }
        #endregion

        #region Fields

        [Test]
        public void ClassFieldNameIsExactMatch()
        {
            var input = new BasicClass();
            var result = test(input, "Field");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassFieldNameDiffersInCase()
        {
            var input = new BasicClass();
            var result = test(input, "Field");
            Assert.AreEqual("Success!", result);
        }

        [Test]
        public void ClassFieldDoesNotExist()
        {
            var input = new BasicClass();
            var result = test(input, "BlahBlahBlah");
            Assert.Null(result);
        }

        [Test]
        public void StructFieldNameIsExactMatch()
        {
            var input = new BasicStruct("Yeah!");
            var result = test(input, "Field");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructFieldNameDiffersInCase()
        {
            var input = new BasicStruct("Yeah!");
            var result = test(input, "Field");
            Assert.AreEqual("Yeah!", result);
        }

        [Test]
        public void StructFieldDoesNotExist()
        {
            var input = new BasicStruct("Yeah!");
            var result = test(input, "BlahBlahBlah");
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

            var result = test(input, "potentially_AMBIGIOUS_field");
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

            var result = test(input, "POTENTIALLY_AMBIGIOUS_FIELD");
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

            var result = test(input, "potentially_ambigious_field");
            Assert.AreEqual("chips", result);
        }

        [Test]
        public void PrivateField()
        {
            var input = new PrivateMembers();

            var result = test(input, "_privateField");
            Assert.Null(result);
        }
        [Test]
        public void StaticField()
        {
            var input = new StaticMembers();

            var result = test(input, "StaticField");
            Assert.Null(result);
        }

        #endregion

        #region Methods
        [Test]
        public void PublicMethodWithZeroArgumentsIsInvokedAsMember()
        {
            var input = new MethodsWith0Parameters();

            var result = test(input, "DoSomething");
            Assert.AreEqual("Hello World", result);
        }

        [Test]
        public void PrivateMethodWithZeroArgumentsIsNotInvokedAsMember()
        {
            var input = new MethodsWith0Parameters();

            var result = test(input, "Secret");
            Assert.IsNull(result);
        }

        [Test]
        public void StaticMethodWithZeroArgumentsIsNotInvokedAsMember()
        {
            var input = new MethodsWith0Parameters();

            var result = test(input, "Static");
            Assert.IsNull(result);
        }

        #endregion


        [Test]
        public void NullInput()
        {
            object input = null;
            var result = test(input, "Field");
            Assert.Null(result);
        }
        
        [Test]
        public void ShouldReturnEnumValueWhenMemberOnEnumType()
        {
            var input = typeof(UriFormat);
            var result = test(input, "Unescaped");

            Assert.That(result, Is.EqualTo(UriFormat.Unescaped));
        }


        private object test(object input, string memberName)
        {
            var binder = new VelocityGetMemberBinder(memberName);
            return InvokeBinder(binder, input);
        }

        public class BasicClass : IExplicit, IConflict
        {
            public string Field = "Success!";
            public string Property { get { return Field; } }


            string IExplicit.Hidden { get { return "Super Secret"; }}
            string IExplicit.HiddenConflict { get { return "Conflict"; } }
            string IConflict.HiddenConflict { get { return "Conflict"; } }
        }


        public struct BasicStruct
        {
            public BasicStruct(string value)
            {
                Field = value;
            }

            public string Field;
            public string Property { get { return Field; } }

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
            public string DoSomething()
            {
                return "Hello World";
            }

            public static string Static()
            {
                return "Fail";
            }

            private string Secret()
            {
                return "Fail";
            }
        }

    }
}
