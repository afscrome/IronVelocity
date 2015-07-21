using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.Binders.Reuse
{
    [TestFixture]
    public class SetMemberReuseTests : BinderReuseTestBase
    {
        [Test]
        public void SetMemberReuse()
        {
            var expectedOutput = "Hello World";

            for (int i = 0; i < 5; i++)
            {
                var helper = new SetMemberHelper();
                var context = new Dictionary<string, object>{
                    {"x", helper}
                };

                var input = "#set($x.Property = 3)\r\n#set($x.Property = 123)Hello World";

                Utility.TestExpectedMarkupGenerated(input, expectedOutput, context, isGlobalEnvironment: false);
                Assert.AreEqual(123, helper.Property);

            }
            Assert.AreEqual(1, CallSiteBindCount);
        }

        public class SetMemberHelper
        {
            public int Property { get; }
        }
    }
}
