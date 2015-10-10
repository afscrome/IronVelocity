using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution.BinderReuse
{
    [TestFixture(GlobalMode.AsProvided)]
    [TestFixture(GlobalMode.Force)]
    public class GetMemberReuseTests : BinderReuseTestBase
    {
        public GetMemberReuseTests(GlobalMode mode) : base(mode)
        {
        }

        [Test]
        public void GetMemberReuse()
        {
            for (int i = 0; i < 5; i++)
            {
                var context = new Dictionary<string, object>{
                    {"x", "testing 1 2 3"}
                };

                var input = "$x.Length $x.Length";
                var expectedOutput = "13 13";

                var execution = ExecuteTemplate(input, context);
                Assert.That(execution.Output, Is.EqualTo(expectedOutput));
            }

            Assert.That(CallSiteBindCount, Is.EqualTo(1));
        }
    }
}
