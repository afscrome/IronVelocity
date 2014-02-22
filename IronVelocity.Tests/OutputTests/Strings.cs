using NUnit.Framework;
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
