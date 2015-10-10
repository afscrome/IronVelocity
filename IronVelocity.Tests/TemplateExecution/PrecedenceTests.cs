using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class PrecedenceTests : TemplateExeuctionBase
    {
        public PrecedenceTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase("true || true && false", true, TestName = "when_ExecutingAnExpression_AndHasHigherPrecedenceThanOr")]
        [TestCase("false && false == false", false, TestName = "when_ExecutingAnExpression_EqualityHasHigherPrecedenceThanAnd")]
        [TestCase("false == true > false", true, TestName = "when_ExecutingAnExpression_RelationalHasHigherPrecedenceThanEquality")]
        [TestCase("1 > 2 + 3", false, TestName = "when_ExecutingAnExpression_AdditiveHasHigherPrecedenceThanRelational")]
        [TestCase("1 + 2 * 5", 11, TestName = "when_ExecutingAnExpression_MultiplicativeHasHigherPrecedenceThanAdditive")]

        //The following are the same expressions as above, but using parentheses to ensure the first operator is performed first
        //These should all give different results to above
        [TestCase("(true || true) && false", false, TestName = "when_ExecutingAnExpression_ParenthesiedExpressionHasHigherPrecedence1")]
        [TestCase("(false && false) == false", true, TestName = "when_ExecutingAnExpression_ParenthesiedExpressionHasHigherPrecedence2")]
        [TestCase("(false == true) > false", false, TestName = "when_ExecutingAnExpression_ParenthesiedExpressionHasHigherPrecedence3")]
        [TestCase("(1 > 2) + 3", null, TestName = "when_ExecutingAnExpression_ParenthesiedExpressionHasHigherPrecedence4")]
        [TestCase("(1 + 2) * 5", 15, TestName = "when_ExecutingAnExpression_ParenthesiedExpressionHasHigherPrecedence5")]
        public void Test(string input, object expected)
        {
            var result = EvaluateExpression(input);
            Assert.That(result, Is.EqualTo(expected));
        }

    }
}
