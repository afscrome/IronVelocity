using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests
{
    [TestFixture]
    public class TextAndCommentTests : OutputTestBase
    {
        [Test]
        public void TextOnly()
        {
            var input = "Hello World";
            var expected = "Hello World";

            TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void SingleLineCommentIgnored()
        {
            var input = "dd ##bar\r\n";
            var expected = "dd ";

            TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void MultiLineCommentIgnored()
        {
            var input = "foo#*bar*#baz";
            var expected = "foobaz";

            TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void SuccessfullVoidDoesNotOutputOriginalToken()
        {
            var input = "Hello $x.DoStuff()";
            var expected = "Hello ";
            var ctx = new Dictionary<string, object>();
            ctx["x"] = new VoidTest();
            TestExpectedMarkupGenerated(input, expected, ctx);
        }

        public class VoidTest
        {
            public void DoStuff()
            {
            }
        }

    }

  
}
