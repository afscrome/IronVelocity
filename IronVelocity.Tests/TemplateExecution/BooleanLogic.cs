using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.TemplateExecution
{
    public class BooleanLogic : TemplateExeuctionBase
    {
        [TestCase(true, true, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(false, false, false)]
        public void BasicOr(bool left, bool right, bool expectedResult)
        {
            var input = string.Format($"#set($result = {left.ToString().ToLower()} || {right.ToString().ToLower()})");

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(expectedResult));
        }

        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void BasicAnd(bool left, bool right, bool expectedResult)
        {
            var input = string.Format($"#set($result = {left.ToString().ToLower()} && {right.ToString().ToLower()})");

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(expectedResult));

        }

        [TestCase(123, "'hello'", true)]
        [TestCase("'abc'", "$null", true)]
        [TestCase("$null", 63f, true)]
        [TestCase("$null", "$null", false)]
        public void OrCoercion(object left, object right, bool expectedResult)
        {
            var input = string.Format($"#set($result = {left.ToString().ToLower()} || {right.ToString().ToLower()})");

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(expectedResult));
        }

        [TestCase(123, "'hello'", true)]
        [TestCase("'abc'", "$null", false)]
        [TestCase("$null", 63f, false)]
        [TestCase("$null", "$null", false)]
        public void AndCoercion(object left, object right, bool expectedResult)
        {
            var input = string.Format($"#set($result = {left.ToString().ToLower()} && {right.ToString().ToLower()})");

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(expectedResult));
        }

        [Test]
        public void OrSecondOperandNotEvaluatedIfFirstIsTrue()
        {
            var input = "#set($result = true || $helper.Fail())";

            var context = new Dictionary<string, object>{
                ["result" ] = new ShortCircuitHelper()
            };

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(true));
        }

        [Test]
        public void AndSecondOperandNotEvaluatedIfFirstIsFalse()
        {
            var input = "#set($result = false && $helper.Fail())";

            var context = new Dictionary<string, object>
            {
                ["result"] = new ShortCircuitHelper()
            };

            var result = ExecuteTemplate(input);

            Assert.That(result.Context["result"], Is.EqualTo(false));
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
