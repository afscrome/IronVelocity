using NUnit.Framework;
using System.Collections.Generic;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Foreach
    {
        [Test]
        public void RendersNoDataForInvalidEnumerable()
        {
            var input = @"#set($y = false)
#foreach($x in $y)
hello
#noData
Nada
#end";
            var expected = "Nada\r\n";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void RendersNoDataForNullEnumerable()
        {
            var input = @"#foreach($x in $null)
hello
#noData
Nada
#end";
            var expected = "Nada\r\n";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void EnumerableOnlyCalledOnce()
        {
            var test = new ForeachExecutionCountTest();

            var context = new Dictionary<string,object>{
                {"test", test}
            };
            var input = @"#foreach($x in $test.Numbers())$x #end";
            var expected = "1 2 3 ";


            Utility.TestExpectedMarkupGenerated(input, expected, context);
        }

        public class ForeachExecutionCountTest
        {
            bool hasBeenCalled = false;
            public int[] Numbers()
            {
                if (hasBeenCalled)
                    Assert.Fail("Enumerable was executed twice");
                hasBeenCalled = true;

                return new[] { 1,2,3};
            }
        }

    }
}
