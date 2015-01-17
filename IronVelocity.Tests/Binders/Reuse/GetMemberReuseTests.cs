using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.Binders.Reuse
{
    [TestFixture]
    public class GetMemberReuseTests : BinderReuseTestBase
    {
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

                Utility.TestExpectedMarkupGenerated(input, expectedOutput, context, isGlobalEnvironment: false);
            }

            Assert.AreEqual(1, CallSiteBindCount);
        }
    }
}
