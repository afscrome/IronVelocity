using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TextAndCommentTests
    {



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
