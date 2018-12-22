using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class BooleanLogic : TemplateExeuctionBase
    {
        public BooleanLogic(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase(true, true, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(false, false, false)]
        public void BasicOr(bool left, bool right, bool expectedResult)
        {
            var input = $"#set($result = {left.ToString().ToLower()} || {right.ToString().ToLower()})";

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(expectedResult));
        }

        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void BasicAnd(bool left, bool right, bool expectedResult)
        {
            var input = $"#set($result = {left.ToString().ToLower()} && {right.ToString().ToLower()})";

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(expectedResult));
        }

        [TestCase("!true", false)]
        [TestCase("  !false", true)]
        [TestCase("  !  false", true)]
        public void BasicNot(string input, bool expectedResult)
        {
            var result = EvaluateExpression(input);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase(123, "'hello'", true)]
        [TestCase("'abc'", "$null", true)]
        [TestCase("$null", 63f, true)]
        [TestCase("$null", "$null", false)]
        public void OrCoercion(object left, object right, bool expectedResult)
        {
            var result = EvaluateExpression($"{left.ToString().ToLower()} || {right.ToString().ToLower()}");

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase(123, "'hello'", true)]
        [TestCase("'abc'", "$null", false)]
        [TestCase("$null", 63f, false)]
        [TestCase("$null", "$null", false)]
        public void AndCoercion(object left, object right, bool expectedResult)
        {
            var result = EvaluateExpression($"{left.ToString().ToLower()} && {right.ToString().ToLower()}");

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase("'abc'", false)]
        [TestCase("$null", true)]
        public void NotCoercion(object value, bool expectedResult)
        {
            var result = EvaluateExpression($"!{value}");
            Assert.That(result, Is.EqualTo(expectedResult));
        }

		[Test]
        public void OrSecondOperandNotEvaluatedIfFirstIsTrue()
        {
			var context = new {
				helper = new ShortCircuitHelper()
			};

			var result = EvaluateExpression("true || $helper.Fail()", context);

            Assert.That(result, Is.True);
        }

        [Test]
        public void AndSecondOperandNotEvaluatedIfFirstIsFalse()
        {
			var context = new {
				helper = new ShortCircuitHelper()
			};

            var result = EvaluateExpression("false && $helper.Fail()", context);

            Assert.That(result, Is.False);
        }

        private class ShortCircuitHelper
        {
            public void Fail()
            {
                Assert.Fail("Second operand was executed when it shouldn't have been");
            }
        }
    }
}
