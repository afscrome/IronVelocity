using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    /// <summary>
    /// This class tests that we are properly unboxing any boxed value types before accessing child members which may mutate the value type
    /// 
    /// Remember that value types (primitives & structs) are passed by value rather than by reference.  
    /// If we fail to properly unbox an object before calling a member which mutates the value type
    /// we will be modifying the boxed copy, and not the underlying value type.
    /// </summary>
    public class BoxingTests : TemplateExeuctionBase
    {

        [Test]
        public void ShouldNotBoxBeforeInvokingPropertyOnValueType()
        {
            // If $x is boxed each time a property is invoked, the property is invoked on the copy
            // So any state mutations won't persist for the next use of $x
            var input = "$x.CallCount, $x.CallCount, $x.CallCount";
            var expectedOutput = "0, 1, 2";
            var expectedCallCountValue = 3;

            var context = new Dictionary<string, object>
            {
                ["x"] = new TestStruct(),
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo(expectedOutput));
            var x = (TestStruct)context["x"];
            Assert.That(x.CallCount, Is.EqualTo(expectedCallCountValue));
        }


        [Test]
        public void ShouldNotBoxBeforeSettingPropertyOnValueType()
        {
            // If $x is boxed each time before being set, the property is set on the copy
            // So the next use of $x will be unchanged by the #set.

            var input = "#set($x.ManualInt = 5)$x.ManualInt";
            var expectedOutput = "5";
            var expectedManualIntValue = 5;
            var context = new Dictionary<string, object>
            {
                ["x"] = new TestStruct(),
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo(expectedOutput));
            var x = (TestStruct)context["x"];
            Assert.That(x.ManualInt, Is.EqualTo(expectedManualIntValue));
        }

        [Test]
        public void ShouldNotBoxValueTypeBeforeInvokingMethod()
        {
            // If $x is boxed each time a method is invoked, the method is invoked on the copy
            // So any state mutations won't persist for the next use of $x
            var input = "$x.GetCallCount(), $x.GetCallCount(), $x.GetCallCount()";
            var expectedOutput = "0, 1, 2";
            var expectedCallCount = 3;
            var context = new Dictionary<string, object>
            {
                ["x"] = new TestStruct(),
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo(expectedOutput));
            var x = (TestStruct)context["x"];
            Assert.That(x.GetCallCount(), Is.EqualTo(expectedCallCount));
        }

        public struct TestStruct
        {
            private int _callCount;

            public int GetCallCount() => _callCount++;
            public int CallCount => _callCount++; 
            public int ManualInt { get; set; }
        }


    }
}
