using NUnit.Framework;

namespace Tests.Numeric
{
    [TestFixture]
    public class SubtractionTests
    {
        [Test]
        public void BasicSubtraction()
        {
            var input = "#set($x = 6 - 9)$x";
            var expected = "-3";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void Subtraction_Null_LeftHandSide()
        {
            var input = "#set($x = $null - 2)$x";
            var expected = "";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void Subtraction_Null_RightHandSide()
        {
            var input = "#set($x = 1 - $null)$x";
            var expected = "";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void Subtraction_Null_BothSides()
        {
            var input = "#set($x = $null - $null)$x";
            var expected = "";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

    }
}
