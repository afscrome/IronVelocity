using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution.BinderReuse
{
    [TestFixture(GlobalMode.AsProvided)]
    [TestFixture(GlobalMode.Force)]
    public class SetMemberReuseTests : BinderReuseTestBase
    {
        public SetMemberReuseTests(GlobalMode mode) : base(mode)
        {
        }

        [Test]
        public void SetMemberReuse()
        {
            var expectedOutput = "Hello World";

            for (int i = 0; i < 5; i++)
            {
                var helper = new SetMemberHelper();
                var context = new { x = helper };

                var input = "#set($x.Property = 3)\r\n#set($x.Property = 123)Hello World";

                var execution = ExecuteTemplate(input, context);

                Assert.That(execution.Output, Is.EqualTo(expectedOutput));
                Assert.That(helper.Property, Is.EqualTo(123));

            }
            Assert.That(CallSiteBindCount, Is.EqualTo(1));
        }

        public class SetMemberHelper
        {
            public int Property { get; set; }
        }
    }
}
