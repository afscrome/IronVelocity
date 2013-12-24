using NUnit.Framework;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Method
    {
        [Test]
        public void ParameterlessMethod()
        {
            var input = "#set($x = 123)$x.ToString()";
            var expected = "123";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }


    }
}
