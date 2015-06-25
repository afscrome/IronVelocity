using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests
{
    /// <summary>
    /// This class tests that we are properly unboxing any boxed value types before accessing child members which may mutate the value type
    /// 
    /// Remember that value types (primitives & structs) are passed by value rather than by reference.  
    /// If we fail to properly unbox an object before calling a member which mutates the value type
    /// we will be modifying the boxed copy, and not the underlying value type.
    /// </summary>
    public class BoxingTests
    {
        
        [Test]
        public void BoxTestWithProperyGet()
        {
            var context = new Dictionary<string, object>();
            context["x"] = new TestStruct();
            Utility.TestExpectedMarkupGenerated("$x.CallCount, $x.CallCount, $x.CallCount", "0, 1, 2", context);


            if (Utility.DefaultToGlobals)
            {
                Assert.Inconclusive("TODO: How can we test the rest with static globals? Is it even possible (or valuable)?");
            }

            var x = (TestStruct)context["x"];
            Assert.AreEqual(3, x.CallCount);
        }


        [Test]
        public void BoxTestWithProperyAssignment()
        {
            //Currently fails because $x is being boxed when returned from the Set expression, so we're actually 
            // setting the value on a copy of the object, not the original

            var context = new Dictionary<string, object>();
            context["x"] = new TestStruct();
            Utility.TestExpectedMarkupGenerated("#set($x.ManualInt = 5)$x.ManualInt", "5", context);

            if (Utility.DefaultToGlobals)
            {
                Assert.Inconclusive("TODO: How can we test the rest with static globals? Is it even possible (or valuable)?");
            }

            var x = (TestStruct)context["x"];
            Assert.AreEqual(5, x.ManualInt);
        }

        [Test]
        public void BoxTestWithMethodInternalMutation()
        {
            var context = new Dictionary<string, object>();
            context["x"] = new TestStruct();
            Utility.TestExpectedMarkupGenerated("$x.GetCallCount(), $x.GetCallCount(), $x.GetCallCount()", "0, 1, 2", context);


            if (Utility.DefaultToGlobals)
            {
                Assert.Inconclusive("TODO: How can we test the rest with static globals? Is it even possible (or valuable)?");
            }

            var x = (TestStruct)context["x"];
            Assert.AreEqual(3, x.CallCount);
        }

        public struct TestStruct
        {
            private int _callCount;
            public int GetCallCount() { return _callCount++; }
            public int CallCount{get { return _callCount++; }}

            private int _manualInt;
            public int ManualInt {
                get { return _manualInt; }
                set { _manualInt = value; }
            }
        }


    }
}
