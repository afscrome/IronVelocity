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

        [Test, Ignore("Seems to be bug in NVelocity")]
        public void SingleLineCommentIgnored()
        {
            //TODO: investigate failure
            //Seems to be a bug in nvelocity
            var input = "dd ##bar";
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
