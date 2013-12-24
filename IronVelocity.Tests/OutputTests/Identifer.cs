using NUnit.Framework;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Identifer
    {
        [Test]
        public void RenderProperty()
        {
            var input = "#set($x = 'hello world')$x.Length";
            var expected = "11";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void RenderPropertyWrongCase()
        {
            var input = "#set($x = 'hello world')$x.LeNgTh";
            var expected = "11";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void SetProperty()
        {
            var input = "#set($x = 'foo')#set($y = $x.Length)$y";
            var expected = "3";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }



        [Test]
        public void RenderPropertyDifferentTypes()
        {
            var input = "#set($x = 'foo')$x.Length #set($x= 123)$x.Length #set($x = 'foobar')";
            var expected = "3";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }


        [Test]
        public void NonExistantProperty()
        {
            var input = "#set($x = 'hello world')$x.IDontExist";
            var expected = "";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

    }
}
