using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Tests;

namespace IronVelocity.Tests.Binders.Reuse
{
    [TestFixture]
    public class InvokeMemberReuseTests : BinderReuseTestBase
    {
        [Test]
        public void InvokeMemberReuse()
        {
            for (int i = 0; i < 5; i++)
            {
                var context = new Dictionary<string, object>{
                    {"x", 123}
                };

                var input = "$x.ToString() $x.ToString()";
                var expectedOutput = "123 123";

                Utility.TestExpectedMarkupGenerated(input, expectedOutput, context, isGlobalEnvironment: false);
                Assert.AreEqual(1, CallSiteBindCount, 1);
            }
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseDifferenceSignaturesWithConstants()
        {
            for (int i = 0; i < 5; i++)
            {
                var context = new Dictionary<string, object>{
                    {"x", new InvokeMemberHelper()}
                };

                var input = "$x.Double(123) $x.Double('hello')";
                var expectedOutput = "246 hellohello";

                Utility.TestExpectedMarkupGenerated(input, expectedOutput, context, isGlobalEnvironment: false);
                Assert.AreEqual(2, CallSiteBindCount);
            }
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseDifferenceSignaturesWithVariables()
        {
            for (int i = 0; i < 5; i++)
            {
                var context = new Dictionary<string, object>{
                    {"x", new InvokeMemberHelper()},
                    {"a", "hello"},
                    {"b", 123}
                };

                var input = "$x.Double($a) $x.Double($b)";
                var expectedOutput = "hellohello 246";

                Utility.TestExpectedMarkupGenerated(input, expectedOutput, context, isGlobalEnvironment: false);
                Assert.AreEqual(2, CallSiteBindCount);
            }
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseCompatibleSignatures1()
        {
            for (int i = 0; i < 5; i++)
            {
                var context = new Dictionary<string, object>{
                    {"x", new InvokeMemberHelper()},
                    {"y", 1.3}
                };

                var input = "$x.Double($y) $x.Double(123)";
                var expectedOutput = "2.6 246";

                Utility.TestExpectedMarkupGenerated(input, expectedOutput, context, isGlobalEnvironment: false);
                Assert.AreEqual(2, CallSiteBindCount);
            }
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseCompatibleSignatures2()
        {
            for (int i = 0; i < 5; i++)
            {
                var context = new Dictionary<string, object>{
                    {"x", new InvokeMemberHelper()},
                    {"y", 1.3}
                };

                var input = "$x.Double(123) $x.Double($y)";
                var expectedOutput = "246 2.6";

                Utility.TestExpectedMarkupGenerated(input, expectedOutput, context, isGlobalEnvironment: false);
                Assert.AreEqual(2, CallSiteBindCount);
            }

        }


        public class InvokeMemberHelper
        {
            public double Double(double value)
            {
                return value * 2;
            }

            public string Double(string value)
            {
                return value.ToString() + value.ToString();
            }
        }
    }
}
