using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class TemplateTests : TemplateExeuctionBase
    {
        public TemplateTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase("$('#$x')")]
        [TestCase("$.ajax()")]
        [TestCase("$('#some-id)")]
        public void ShouldRenderJQueryScriptAsIs(string input)
        {
            var context = new { id = "myId" };
            var execution = ExecuteTemplate(input);
            Assert.That(execution.Output, Is.EqualTo(input));
        }

        [Test]
        public void ShouldRenderJQueryCallsAsIs()
        {
            var input = "$('#$id')";
            var context = new { id = "myId" };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Output, Is.EqualTo("$('#myId')"));
        }

        [Test, Ignore("TODO: #set preceedign whitespace consumption")]
        public void ShouldNotHaveConflictsWhenVariableChangesTypeAndPropertyWithSameNameInvoked()
        {
            var input = "#set($x = 'foo')$x.Length #set($x= 123)$x.Length #set($x = 'foobar')$x.length";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Output, Is.EqualTo("3$x.Length6"));
        }

        [Test]
        public void ShouldRenderMixedTextAndReferences()
        {
            var input = "Welcome back $surname, $forename";
            var context = new
            {
                Forename = "John",
                Surname = "Smith"
            };

            var execution = ExecuteTemplate(input, context);

            Assert.That(execution.Output, Is.EqualTo("Welcome back Smith, John"));
        }

    }
}
