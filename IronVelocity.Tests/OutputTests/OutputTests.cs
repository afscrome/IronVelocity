using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TextAndCommentTests
    {

        [Test]
        public void Method()
        {
            var input = "#set($x = \"hello\")$x.ToString()";
            var expected = "llo";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }


        [Test]
        public void TextOnly()
        {
            var input = "Hello World";
            var expected = "Hello World";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void SingleLineCommentIgnored()
        {
            //TODO: investigate failure
            var input = "dd ## bar";
            var expected = "foo";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void MultiLineCommentIgnored()
        {
            var input = "foo#*bar*#baz";
            var expected = "foobaz";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }


    }

  
}
