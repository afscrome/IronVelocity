using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    public class OutputTests : TemplateExeuctionBase
    {
        [TestCase("Hello World")]
        [TestCase("With\r\nNewlines")]
        public void SimpleText(string input)
        {
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(input));
        }

        [TestCase("##Boo", "")]
        [TestCase("#*ya*#", "")]
        [TestCase("Hello##World", "Hello")]
        [TestCase("Hello #*Cruel*# World", "Hello  World")]
        [TestCase("##Hello\r\nWorld", "\r\nWorld")]
        [TestCase("Hello##World\r\n", "Hello\r\n")]
        [TestCase("Hello #* #* very *# Cruel*# World", "Hello  World")]
        public void OutputForCommentMixedWithText(string input, string expectedOutput)
        {
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expectedOutput));
        }


        [TestCase("$undefined")]
        [TestCase("$variable.Undefined")]
        [TestCase("$variable.Undefined()")]
        [TestCase("$variable.ToString().AnotherUndefined")]
        [TestCase("$variable.Undefined.AnotherUndefined()")]
        [TestCase("${undefinedFormal}")]
        public void OutputForUnsilencedReferences(string input)
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
        public void OutputForSilencedReferences(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["variable"] = 123
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
        }

        [Test]
        public void OutputForVoidMethodIsEmpty()
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
