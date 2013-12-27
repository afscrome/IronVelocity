using NUnit.Framework;

namespace Tests.Numeric
{
    [TestFixture]
    public class AdditionTests
    {
        [Test]
        public void BasicAddition()
        {
            var input = "#set($x = 1 + 2)$x";
            var expected = "3";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void Addition_Null_LeftHandSide()
        {
            var input = "#set($x = $null + 2)$x";
            var expected = "$x";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void Addition_Null_RightHandSide()
        {
            var input = "#set($x = 1 + $null)$x";
            var expected = "$x";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void Addition_Null_BothSides()
        {
            var input = "#set($x = $null + $null)$x";
            var expected = "$x";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

    }
}
