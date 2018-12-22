using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class CommentTests : TemplateExeuctionBase
    {
        public CommentTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase("##Boo", "")]
        [TestCase("#*ya*#", "")]
        [TestCase("Hello##World", "Hello")]
        [TestCase("Hello #*Cruel*# World", "Hello  World")]
        [TestCase("##Hello\r\nWorld", "World")]
        [TestCase("Hello##World\r\n", "Hello")]
        [TestCase("Hello #* #* very *# Cruel*# World", "Hello  World")]
        public void When_RenderingTextMixedWithComments_Should_IgnoreComments(string input, string expectedOutput)
        {
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expectedOutput));
        }
    }
}
