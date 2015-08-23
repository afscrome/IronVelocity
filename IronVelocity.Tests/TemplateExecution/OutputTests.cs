using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    public class OutputTests : TemplateExeuctionBase
    {
        [TestCase("Hello World")]
        [TestCase("With\r\nNewlines")]
        public void When_RenderingText_Should_RenderAsIs(string input)
        {
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(input));
        }

        [TestCase("$undefined")]
        [TestCase("$variable.Undefined")]
        [TestCase("$variable.Undefined()")]
        [TestCase("$variable.ToString().AnotherUndefined")]
        [TestCase("$variable.Undefined.AnotherUndefined()")]
        [TestCase("${undefinedFormal}")]
        public void When_RenderingAnUndefinedUnsilencedReference_Should_RenderReferenceSourceCode(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["variable"] = 123
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo(input));
        }

        [TestCase("$!undefined")]
        [TestCase("$!variable.Undefined")]
        [TestCase("$!variable.Undefined()")]
        [TestCase("$!variable.ToString().AnotherUndefined")]
        [TestCase("$!variable.Undefined.AnotherUndefined()")]
        [TestCase("$!{undefinedFormal}")]
        public void When_RenderingAnUndefinedSilencedReference_Should_RenderNothing(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["variable"] = 123
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
        }

        [Test]
        public void When_RenderingVoidMethod_Should_OutputNothing()
        {
            var context = new Dictionary<string, object>
            {
                ["test"] = new VoidTest()
            };

            var result = ExecuteTemplate("$test.DoStuff()", context);

            Assert.That(result.Output, Is.Empty);
        }

        public class VoidTest
        {
            public void DoStuff()
            {
            }
        }

        //Behaviour

        //Void method doesn't render
        //Null / undefined reference prints as is        
        //Null / undefined silent reference prints nothing
        //Set prints nothing
        //Set Eats leading / trailing whitespace (todo: how do newlines fit in)
        //What about other directives
        //Comments not rendered

        //Internals
        //Methdod 
    }
}
