using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(GlobalMode.AsProvided)]
    [TestFixture(GlobalMode.Force)]
    public class MathsTests : TemplateExeuctionBase
    {
        public MathsTests(GlobalMode mode) : base(mode)
        {
        }

        [TestCase(1.3, 3.2, 4.5)]
        [TestCase(1, "$null", null)]
        [TestCase("$null", 2.4, null)]
        [TestCase("$null", "$null", null)]
        public void EvaluateBasicAddition(object left, object right, object expected)
            => BasicMathsTest(left, "+", right, expected);


        [TestCase(1.3, 3.2, -1.9)]
        [TestCase(1, "$null", null)]
        [TestCase("$null", 2.4, null)]
        [TestCase("$null", "$null", null)]
        public void EvaluateBasicSubtraction(object left, object right, object expected)
            => BasicMathsTest(left, "-", right, expected);

        [TestCase(1.3, 3.2, 4.16)]
        [TestCase(1, "$null", null)]
        [TestCase("$null", 2.4, null)]
        [TestCase("$null", "$null", null)]
        public void EvaluateBasicMultiplication(object left, object right, object expected)
            => BasicMathsTest(left, "*", right, expected);

        [TestCase(1.3, 3.2, 0.40625)]
        [TestCase(1, "$null", null)]
        [TestCase("$null", 2.4, null)]
        [TestCase("$null", "$null", null)]
        public void EvaluateBasicDivision(object left, object right, object expected)
            => BasicMathsTest(left, "/", right, expected);

        [TestCase(5, 4, 1)]
        [TestCase(1, "$null", null)]
        [TestCase("$null", 2.4, null)]
        [TestCase("$null", "$null", null)]
        public void EvaluateBasicModulo(object left, object right, object expected)
            => BasicMathsTest(left, "%", right, expected);

        private void BasicMathsTest(object left, string @operator, object right, object expected)
        {
            var input = $"$left {@operator} $right";

            var context = new VelocityContext
            {
                ["left"] = left,
                ["right"] = right,
            };

            var result = EvaluateExpression(input, context);
            Assert.That(result, Is.EqualTo(expected).Within(0.0000005));
        }

    }
}
