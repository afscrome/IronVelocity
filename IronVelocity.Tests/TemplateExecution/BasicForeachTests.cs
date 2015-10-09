using NUnit.Framework;
using System.Collections;
using System;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    public class BasicForeachTests : TemplateExeuctionBase
    {
        [Test]
        public void ShouldEnumerateEnumerable()
        {
            var input = "#foreach($x in [1..2])$x,#end";
            var expected = "1,2,";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldRenderNothingForNullEnumerable()
        {
            var input = "#foreach($y in $null)ABC#end";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.Empty);
        }

        [Test]
        public void ShouldRenderNothingForInvalidEnumerable()
        {
            var input = "#foreach($foo in 123)ABC#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.Empty);
        }

        [Test]
        public void ShouldPersistVariableSetInsideForeachDirectiveOutsideDirective()
        {
            var input = "#foreach($temp in [5..3])#set($persist = $temp)#end$persist";
            var result = ExecuteTemplate(input);

            Assert.That(result.Context.Keys, Contains.Item("persist"));
            Assert.That(result.Context["persist"], Is.EqualTo(3));
            Assert.That(result.Output, Is.EqualTo("3"));
        }

        [Test]
        public void ShouldNotLeakCurrentItemWhenSameVariableIsUsedInNestedForeach()
        {
            var input = "#foreach($item in ['A','B'])#foreach($item in [1..3])$item#end:$item||#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo("123:A||123:B||"));
        }

        [Test]
        public void EnumerableOnlyCalledOnce()
        {
            var input = @"#foreach($x in $test)$x #end";
            var expected = "1 2 3 ";
            var test = new CustomEnumerable();
            var context = new Dictionary<string, object>{
                {"test", test}
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        public class CustomEnumerable : IEnumerable
        {
            bool hasBeenCalled = false;

            public IEnumerator GetEnumerator()
            {
                if (hasBeenCalled)
                    Assert.Fail("Enumerable was executed twice");
                hasBeenCalled = true;

                return (new[] { 1, 2, 3 }).GetEnumerator();
            }
        }
    }
}
