using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class IfDirectiveTests
    {
        [Test]
        public void IfTrue()
        {
            var input = "#if(true)foo#end";
            var expected = "foo";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }
        [Test]
        public void IfFalse()
        {
            var input = "#if(false)foo#end";
            var expected = "";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void IfElseTrue()
        {
            var input = "#if(true)foo#else bar#end";
            var expected = "foo";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }
        [Test]
        public void IfElseFalse()
        {
            var input = "#if(false)foo#else bar#end";
            var expected = " bar";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void IfElseIfElseFirstMatch()
        {
            var input = "#if(true)foo#elseif(true)bar#else baz#end";
            var expected = "foo";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void IfElseIfSecondMatch()
        {
            var input = "#if(false)foo#elseif(true)bar#else baz#end";
            var expected = "bar";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void IfElseIfFinalMatch()
        {
            var input = "#if(false)foo#elseif(false)bar#else baz#end";
            var expected = " baz";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }
        [Test]
        public void IfElseIfNoMatch()
        {
            var input = "#if(false)foo#elseif(false)bar#end";
            var expected = "";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

    }

  
}
