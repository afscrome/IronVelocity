using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SetDirectiveTests
    {
        [Test]
        public void BasicIntegerSet()
        {
            var input = "#set($foo = 5)$foo";
            var expectedOutput = "5";

            Utility.TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        public void BasicStringSet()
        {
            var input = "#set($foo = \"hello world\")$foo";
            var expectedOutput = "hello world";

            Utility.TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        public void BasicVariableSet()
        {
            var input = "#set($foo = 8)#set($bar = $foo)$bar";
            var expectedOutput = "8";

            Utility.TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test]
        public void NullVariableSetIgnored()
        {
            var input = "#set($foo = 8)#set($foo = $null)$foo";
            var expectedOutput = "8";

            Utility.TestExpectedMarkupGenerated(input, expectedOutput);
        }

        [Test, Ignore("Not yet supported")]
        public void InterpolatedStringSet()
        {
            var input = "#set($foo = 'Jim')#set($bar = \"Hello $foo\")$bar";
            var expectedOutput = "Hello Jim";

            Utility.TestExpectedMarkupGenerated(input, expectedOutput);
        }

    }

  
}
