﻿using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests
{
    [TestFixture]
    public class SetDirectiveTests : OutputTestBase 
    {
        [Test]
        public void BasicIntegerSet()
        {
            var input = "#set($foo = 5)$foo";
            var expectedOutput = "5";

            TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        public void BasicStringSet()
        {
            var input = "#set($foo = \"hello world\")$foo";
            var expectedOutput = "hello world";

            TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        public void BasicVariableSet()
        {
            var input = "#set($foo = 8)#set($bar = $foo)$bar";
            var expectedOutput = "8";

            TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        public void NullVariableSetIgnored()
        {
            var input = "#set($foo = 8)#set($foo = $null)$foo";
            var expectedOutput = "8";

            Utility.TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        [Ignore("Interpolated strings not yet supported in Antlr Parser")]
        public void InterpolatedStringSet()
        {
            var input = "#set($foo = 'Jim')#set($bar = \"Hello $foo\")$bar";
            var expectedOutput = "Hello Jim";

            TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        public void SetMethodOnlyCalledOnce()
        {
            var x = new TestClass();
            var context = new Dictionary<string, object>();
            context["x"] = x;

            TestExpectedMarkupGenerated("#set($y = $x.GetCallCount()) $y $y", " 0 0", context);
            Assert.AreEqual(1, x.CallCount);
        }

        [Test]
        public void SetPropertyOnlyCalledOnce()
        {
            var x = new TestClass();
            var context = new Dictionary<string, object>();
            context["x"] = x;

            TestExpectedMarkupGenerated("#set($y = $x.CallCount) $y $y", " 0 0", context);
            Assert.AreEqual(1, x.CallCount);
        }


        public class TestClass
        {
            private int _callCount;
            public int GetCallCount() { return _callCount++; }
            public int CallCount { get { return _callCount++; } }
        }

    }

  
}
