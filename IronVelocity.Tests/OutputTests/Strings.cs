using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Strings
    {
        [Test]
        public void JQueryId()
        {
            var input = "jQuery('#$x')";
            var expected = "jQuery('#$x')";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }        
    }
}
