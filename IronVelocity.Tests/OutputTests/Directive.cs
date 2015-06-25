using NUnit.Framework;

namespace IronVelocity.Tests.OutputTests
{
    public class Directive
    {
        [TestCase("#foobar", "#foobar", TestName = "UnknownDirectivePrintedAsIs")]
        [TestCase("#-", "#-", TestName = "DirectiveLikePrintedAsIs")]
        [TestCase("#set($foo=1)\r\n$foo$\r\n", "1$\r\n", TestName = "UnknownDirectivePrintedAsIs")]
        public void Test(string input, string expected)
        {
            Utility.TestExpectedMarkupGenerated(input, expected);
        }
    }
}
