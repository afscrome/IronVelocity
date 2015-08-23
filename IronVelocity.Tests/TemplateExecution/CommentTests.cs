using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution
{
    public class CommentTests : TemplateExeuctionBase
    {
        [TestCase("##Boo", "")]
        [TestCase("#*ya*#", "")]
        [TestCase("Hello##World", "Hello")]
        [TestCase("Hello #*Cruel*# World", "Hello  World")]
        [TestCase("##Hello\r\nWorld", "\r\nWorld")]
        [TestCase("Hello##World\r\n", "Hello\r\n")]
        [TestCase("Hello #* #* very *# Cruel*# World", "Hello  World")]
        public void When_RenderingTextMixedWithComments_Should_IgnoreComments(string input, string expectedOutput)
        {
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expectedOutput));
        }
    }
}
