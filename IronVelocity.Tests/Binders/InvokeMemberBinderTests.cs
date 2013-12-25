using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using VelocityExpressionTree.Binders;

namespace IronVelocity.Tests.Binders
{
    public class InvokeMemberBinderTests
    {

        [Test]
        public void NullInput()
        {
            object input = null;
            var result = test(input, "DoStuff");
            Assert.Null(result);
        }



        [Test]
        public void MethodOnPrimative()
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
        //null input returns null
        //Void returns null??

        private object test(object input, string methodName, params object[] paramaters)
        {
            var binder = new VelocityInvokeMemberBinder(methodName, new CallInfo(paramaters.Length));

            var value = Expression.Constant(input);
            var expression = Expression.Dynamic(binder, typeof(object), value);

            var action = Expression.Lambda<Func<object>>(expression)
                .Compile();

            return action();
        }

        public class MethodTests
        {
            public string StringResult() { return "hello world"; }

            private string TopSecret() { return "The password is ********"; }
        }

    }
}
