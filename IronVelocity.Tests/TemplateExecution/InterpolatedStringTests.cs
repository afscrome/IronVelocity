using NUnit.Framework;


namespace IronVelocity.Tests.TemplateExecution
{
    public class InterpolatedStringTests : TemplateExeuctionBase
    {
        [Test]
        public void ShouldProcessInterpolatedStringWithOnlyText()
        {
            var input = "#set($result = \"Hello World\")";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("Hello World"));
        }

        [Test]
        public void ShouldProcessInterpolatedStringWithUndefinedReference()
        {
            var input = "#set($result = \"$value\")";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("$value"));
        }

       [Test]
        public void ShouldProcessInterpolatedStringWithUndefinedSilentReference()
        {
            var input = "#set($result = \"$!value\")";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void ShouldProcessInterpolatedStringWithDefinedVariable()
        {
            var input = "#set($result = \"$value\")";
            var context = new { value = 1234 };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("1234"));
        }

        [Test]
        public void ShouldProcessInterpolatedStringWithMixedTextAndReference()
        {
            var input = "#set($result = \"Welcome back $name\")";
            var context = new { name = "John Smith" };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("Welcome back John Smith"));
        }

        [Test]
        public void ShouldProcessInterpolatedStringWithMixedTextAndUndefinedReference()
        {
            var input = "#set($result = \"Welcome back $name\")";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("Welcome back $name"));
        }


        [Test]
        public void ShouldProcessInterpolatedStringWithMixedTextAndUndefinedSilentReference()
        {
            var input = "#set($result = \"Welcome back $!name\")";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("Welcome back "));
        }

        [Test]
        public void ShouldProcessInterpolatedStringWithIfDirective()
        {
            var input = "#set($result = \"#if(true)Hello#end\")";
            var context = new { value = 1234 };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];

            Assert.That(result, Is.EqualTo("Hello"));
        }

        [Test]
        public void ShouldProcessInterpolatedStringWithNestedIfDirective()
        {
            var input = "#set($result = \"#if(true)#if(false) Hello #else World #end#end\")";
            var context = new { value = 1234 };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];

            Assert.That(result, Is.EqualTo(" World "));
        }

        [Test]
        public void ShouldProcessInterpolatedStringWithForeachDirective()
        {
            var input = "#set($result = \"#foreach($y in [1,2,3,4])$y #end\")";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];

            Assert.That(result, Is.EqualTo("1 2 3 4 "));
        }
    }
}
