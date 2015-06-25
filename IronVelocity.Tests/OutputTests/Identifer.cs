using NUnit.Framework;
using System.Collections.Generic;

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
            var input = "$x.LeNgTh";
            var expected = "11";

            var context = new Dictionary<string, object>{
                { "x", "hello world"}
            };

            Utility.TestExpectedMarkupGenerated(input, expected, context);
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
            var input = "#set($x = 'foo')$x.Length #set($x= 123)$x.Length #set($x = 'foobar')$x.length";
            var expected = "3$x.Length6";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }


        [Test]
        public void NonExistantProperty()
        {
            var input = "#set($x = 'hello world')$x.IDontExist";
            var expected = "$x.IDontExist";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

    }
}
