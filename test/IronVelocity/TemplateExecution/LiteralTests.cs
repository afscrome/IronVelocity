using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class LiteralTests : TemplateExeuctionBase
    {
        public LiteralTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase("#[[]]")]
        [TestCase("#[[Foo]]")]
        [TestCase("#[[$ref]]")]
        [TestCase("#[[#directive]]")]
        [TestCase("#[[${bad #{syntax]]")]
        [TestCase("#[[ Single Close ] Square ]]")]
        [TestCase("#[[ Double ] Close ] Square ]]")]
        public void ShouldOutputLiteralContent(string input)
        {
            var expected = input.Substring(3, input.Length - 5);
            var execution = ExecuteTemplate(input);

            Assert.That(execution.Output, Is.EqualTo(expected));
        }
    }
}
