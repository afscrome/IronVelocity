using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Dynamic;
using System.Linq;
using IronVelocity.Tests;

namespace IronVelocity.Tests.Binders
{
    public class InvokeMemberBinderTests : BinderTestBase
    {
        // Overloading rules http://csharpindepth.com/Articles/General/Overloading.aspx
        // http://msdn.microsoft.com/en-us/library/aa691336(v=vs.71).aspx

        [Test]
        public void NullInput()
        {
            object input = null;
            var result = test(input, "DoStuff");
            Assert.Null(result);
        }



        [Test]
        public void MethodOnPrimitive()
        {
            var input = 472;
            var result = test(input, "ToString");
            Assert.AreEqual("472", result);
        }

        [Test]
        public void PrivateMethod()
        {
            object input = null;
            var result = test(input, "TopSecret");
            Assert.Null(result);
        }

        [Test]
        public void InternalMethod()
        {
            object input = null;
            var result = test(input, "Internal");
            Assert.Null(result);
        }


        [Test]
        public void MethodNameNotExist()
        {
            object input = null;
            var result = test(input, "DoStuff");
            Assert.Null(result);
        }

        [Test]
        public void MethodNameExactMatch()
        {
            var input = new MethodTests();
            var result = test(input, "StringResult");
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public void BasicMethodDiffersInCase()
        {
            var input = new MethodTests();
            var result = test(input, "stringRESULT");
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public void MethodNamePartialMatch()
        {
            object input = null;
            var result = test(input, "StringRes");
            Assert.Null(result);
        }

        [Test]
        public void MethodOneParamaterExactTypeMatch()
        {
            var target = new MethodTests();
            var param1 = new object();
            var result = test(target, "OneParameter", param1);

            Assert.AreEqual("One Param Success", result);
        }

        [Test]
        public void MethodOneParamaterAssignableTypeMatch()
        {
            var target = new MethodTests();
            var param1 = "hello world";
            var result = test(target, "OneParameter", param1);

            Assert.AreEqual("One Param Success", result);
        }

        [Test]
        public void ParamArrayWithNoArguments()
        {
            var target = new MethodTests();
            var result = test(target, "ParamArray");

            Assert.AreEqual(0, result);
        }

        [Test]
        public void ParamArrayWithOneArgument()
        {
            var target = new MethodTests();
            var result = test(target, "ParamArray", "one");

            Assert.AreEqual(1, result);
        }

        [Test]
        public void ParamArrayWithTwoArguments()
        {
            var target = new MethodTests();
            var result = test(target, "ParamArray", "one", "two");

            Assert.AreEqual(2, result);
        }

        [Test]
        public void ParamArrayWithArrayArguments()
        {
            var target = new MethodTests();
            var arg1 = new string[] {"one", "two"};
            var result = test(target, "ParamArray", new[] { arg1 });

            Assert.AreEqual(2, result);
        }


        [Test]
        public void ParamArrayWithOneNullArgument()
        {
            var target = new MethodTests();
            var result = test(target, "ParamArray", new object[] { null} );

            Assert.AreEqual(1, result);
        }

        [Test]
        public void ParamArrayWithTwoNullArguments()
        {
            var target = new MethodTests();
            var result = test(target, "ParamArray", null, null);

            Assert.AreEqual(2, result);
        }



        [Test]
        public void PropertyInvokedForMethodWithNoArguments()
        {
            var target = new MethodTests();
            var result = test(target, "Property");

            Assert.AreEqual("A property", result);
        }
        [Test]
        public void FieldInvokedForMethodWithNoArguments()
        {
            var target = new MethodTests();
            var result = test(target, "Field");

            Assert.AreEqual("Marshal", result);
        }


        //null input returns null
        //Void returns null??

        private object test(object input, string methodName, params object[] paramaters)
        {
            var binder = new VelocityInvokeMemberBinder(methodName, new CallInfo(paramaters.Length));

            var args = new[] { input }.Concat(paramaters).ToArray();

            return InvokeBinder(binder, args);
        }

        public class AmbigiousMethods
        {
            public int Ambigious(string param1, object param2)
            {
                return -1;
            }
            public string Ambigious(object param1, string param2)
            {
                return "fail";
            }
            public float Ambigious(object param1, object param2)
            {
                return 0.5f;
            }
        }

        #region Overload Suitability
        [Test]
        public void OverloadSuitability_Object()
        {
            OverloadSuitabilityTests<object>(-1);
        }
        [Test]
        public void OverloadSuitability_Parent()
        {
            OverloadSuitabilityTests<Parent>("Failure");
        }
        [Test]
        public void OverloadSuitability_Child()
        {
            OverloadSuitabilityTests<Child>(true);
        }

        [Test]
        public void OverloadSuitability_Son()
        {
            OverloadSuitabilityTests<Son>(Guid.Empty);
        }

        [Test]
        public void OverloadSuitability_Daughter()
        {
            OverloadSuitabilityTests<Daughter>(true);
        }

        public void OverloadSuitabilityTests<T>(object expectedResult)
            where T : new()
        {
            var input = new OverloadSuitability();
            var result = test(input, "Overload", new T());

            Assert.AreEqual(expectedResult, result);
        }

        public class OverloadSuitability
        {
            public int Overload(object param) { return -1; }
            public string Overload(Parent param) { return "Failure"; }
            public bool Overload(Child param) { return true; }
            public Guid Overload(Son param) { return Guid.Empty; }
        }

        #endregion



        public class MethodTests
        {
            public string StringResult() { return "hello world"; }

            private string TopSecret() { return "The password is ********"; }

            internal string Internal() { return "Access code: *****"; }

            public object OneParameter(object param)
            {
                return "One Param Success";
            }

            public int ParamArray(params object[] values)
            {
                return values.Length;
            }

            public string Property { get { return "A property"; } }
            public string Field = "Marshal";

        }


    }
}
