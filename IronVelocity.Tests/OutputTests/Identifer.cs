using NUnit.Framework;
using System.Collections.Generic;
using IronVelocity.Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Identifer
    {

        [Test]
        public void RenderPropertyDifferentTypes()
        {
            var input = "#set($x = 'foo')$x.Length #set($x= 123)$x.Length #set($x = 'foobar')$x.length";
            var expected = "3$x.Length6";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }


    }
}
