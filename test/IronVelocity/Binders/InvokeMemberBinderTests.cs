﻿using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Dynamic;
using System.Linq;

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
            var result = Invoke(input, "DoStuff");
            Assert.Null(result);
        }



        [Test]
        public void MethodOnPrimitive()
        {
            var input = 472;
            var result = Invoke(input, "ToString");
            Assert.AreEqual("472", result);
        }

        [Test]
        public void PrivateMethod()
        {
            object input = null;
            var result = Invoke(input, "TopSecret");
            Assert.Null(result);
        }

        [Test]
        public void InternalMethod()
        {
            object input = null;
            var result = Invoke(input, "Internal");
            Assert.Null(result);
        }


        [Test]
        public void MethodNameNotExist()
        {
            object input = null;
            var result = Invoke(input, "DoStuff");
            Assert.Null(result);
        }

        [Test]
        public void MethodNameExactMatch()
        {
            var input = new MethodTests();
            var result = Invoke(input, "StringResult");
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public void BasicMethodDiffersInCase()
        {
            var input = new MethodTests();
            var result = Invoke(input, "stringRESULT");
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public void MethodNamePartialMatch()
        {
            object input = null;
            var result = Invoke(input, "StringRes");
            Assert.Null(result);
        }

        [Test]
        public void MethodOneParamaterExactTypeMatch()
        {
            var target = new MethodTests();
            var param1 = new object();
            var result = Invoke(target, "OneParameter", param1);

            Assert.AreEqual("One Param Success", result);
        }

        [Test]
        public void MethodOneParamaterAssignableTypeMatch()
        {
            var target = new MethodTests();
            var param1 = "hello world";
            var result = Invoke(target, "OneParameter", param1);

            Assert.AreEqual("One Param Success", result);
        }

        [Test]
        public void ParamArrayWithNoArguments()
        {
            var target = new MethodTests();
            var result = Invoke(target, "ParamArray");

            Assert.AreEqual(0, result);
        }

        [Test]
        public void ParamArrayWithOneArgument()
        {
            var target = new MethodTests();
            var result = Invoke(target, "ParamArray", "one");

            Assert.AreEqual(1, result);
        }

        [Test]
        public void ParamArrayWithTwoArguments()
        {
            var target = new MethodTests();
            var result = Invoke(target, "ParamArray", "one", "two");

            Assert.AreEqual(2, result);
        }

        [Test]
        public void ParamArrayWithArrayArguments()
        {
            var target = new MethodTests();
            var arg1 = new string[] {"one", "two"};
            var result = Invoke(target, "ParamArray", new[] { arg1 });

            Assert.AreEqual(2, result);
        }

        [Test]
        public void ParamArrayWithNullArgument()
        {
            var target = new MethodTests();
            var result = Invoke(target, "ParamArray", new object[] { null });

            Assert.IsNull(result);
        }
        [Test]
        public void ParamArrayWithArrayWithOneNullArgument()
        {
            var target = new MethodTests();
            var result = Invoke(target, "ParamArray", new object[] { new object[1] });

            Assert.AreEqual(1, result);
        }

        [Test]
        public void ParamArrayWithTwoNullArguments()
        {
            var target = new MethodTests();
            var result = Invoke(target, "ParamArray", null, null);

            Assert.AreEqual(2, result);
        }

		[Test]
		public void ShouldInvokeMemberOnDynamicObject()
		{
			dynamic input = new ExpandoObject();
			input.Double = new Func<int, int>(x => x * 2);

			var result = Invoke(input, "Double", 123);
			Assert.That(result, Is.EqualTo(246));
		}


		[Test]
        [Ignore("NVelocity Specific functionality")]
        public void PropertyInvokedForMethodWithNoArguments()
        {
            var target = new MethodTests();
            var result = Invoke(target, "Property");

            Assert.AreEqual("A property", result);
        }

        [Test]
        [Ignore("NVelocity Specific functionality")]
        public void FieldInvokedForMethodWithNoArguments()
        {
            var target = new MethodTests();
            var result = Invoke(target, "Field");

            Assert.AreEqual("Marshal", result);
        }


        //null input returns null
        //Void returns null??

        private object Invoke(object input, string methodName, params object[] paramaters)
        {
            var binder = new VelocityInvokeMemberBinder(methodName, new CallInfo(paramaters.Length), new MethodResolver(new OverloadResolver(new ArgumentConverter())));

            var args = new[] { input }.Concat(paramaters).ToArray();

            return InvokeBinder(binder, args);
        }

        public class AmbigiousMethods
        {
            public int Ambigious(string param1, object param2) => -1;
            public string Ambigious(object param1, string param2) => "fail";
            public float Ambigious(object param1, object param2) => 0.5f;
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
            var result = Invoke(input, "Overload", new T());

            Assert.AreEqual(expectedResult, result);
        }

        public class OverloadSuitability
        {
            public int Overload(object param) => -1;
            public string Overload(Parent param) => "Failure";
            public bool Overload(Child param) => true;
            public Guid Overload(Son param) => Guid.Empty;
        }

        #endregion

        public class MethodTests
        {
            public string StringResult() => "hello world";
            private string TopSecret() => "The password is ********";
            internal string Internal() => "Access code: *****";
            public object OneParameter(object param) => "One Param Success";
            public int? ParamArray(params object[] values) => values?.Length;
            public string Property => "A property";
            public string Field = "Marshal";
        }


    }
}
