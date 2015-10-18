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
    public class StringTests : TemplateExeuctionBase
    {
        public StringTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase("'Hello World'", "Hello World")]
        [TestCase("'Alice''s foot'", "Alice's foot")]
        [TestCase("'$foo'", "$foo")]
        [TestCase("'#if(true)Test#end'", "#if(true)Test#end")]
        [TestCase("'\"Morning\" he said'", "\"Morning\" he said")]
        public void ShouldEvaluateString(string input, string expected)
        {
            input = $"#set($result = {input})";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Output, Is.Empty);
            Assert.That(execution.Context.Keys, Contains.Item("result"));
            Assert.That(execution.Context["result"], Is.EqualTo(expected));

        }
    }
}
