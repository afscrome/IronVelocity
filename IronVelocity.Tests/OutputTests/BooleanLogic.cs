using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.OutputTests
{
    public class BooleanShortCircuiting
    {
        
        [TestCase(true, true, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(false, false, false)]
        public void BasicOr(bool left, bool right, bool result)
        {
            var input = string.Format("#if({0} || {1})true#else\r\nfalse#end", left.ToString().ToLower(), right.ToString().ToLower());
            var expected = result ? "true" : "false";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }


        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void BasicAnd(bool left, bool right, bool result)
        {
            var input = string.Format("#if({0} && {1})true#else\r\nfalse#end", left.ToString().ToLower(), right.ToString().ToLower());
            var expected = result ? "true" : "false";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [TestCase(123, "'hello'", true)]
        [TestCase("'abc'", "$null", true)]
        [TestCase("$null", 63f, true)]
        [TestCase("$null", "$null", false)]
        public void OrCoercion(object left, object right, bool result)
        {
            var input = string.Format("#if({0} || {1})true#else\r\nfalse#end", left.ToString().ToLower(), right.ToString().ToLower());
            var expected = result ? "true" : "false";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [TestCase(123, "'hello'", true)]
        [TestCase("'abc'", "$null", false)]
        [TestCase("$null", 63f, false)]
        [TestCase("$null", "$null", false)]
        public void AndCoercion(object left, object right, bool result)
        {
            var input = string.Format("#if({0} && {1})true#else\r\nfalse#end", left.ToString().ToLower(), right.ToString().ToLower());
            var expected = result ? "true" : "false";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void OrSecondOperandNotEvaluatedIfFirstIsTrue()
        {
            var input = "#if(true || $x.Fail())pass#else\r\nfail#end";
            var expected = "pass";

            var context = new Dictionary<string, object>{
                { "x", new ShortCircuitHelper()}
            };
            Utility.TestExpectedMarkupGenerated(input, expected, context);
        }

        [Test]
        public void AndSecondOperandNotEvaluatedIfFirstIsFalse()
        {
            var input = "#if(false && $x.Fail())fail#else\r\npass#end";
            var expected = "pass";

            var context = new Dictionary<string, object>{
                { "x", new ShortCircuitHelper()}
            };

            Utility.TestExpectedMarkupGenerated(input, expected, context);
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
