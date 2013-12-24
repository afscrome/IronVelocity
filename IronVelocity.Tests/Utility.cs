using IronVelocity;
using NUnit.Framework;
using System.Text;

namespace Tests
{
    public static class Utility
    {
        public static void TestExpectedMarkupGenerated(string input, string expectedOutput)
        {
            var action = IronVelocityBuilder.BuildExpressionTree(input).Compile();

            var builder = new StringBuilder();
            action(builder);

            Assert.AreEqual(expectedOutput, builder.ToString());
        }
    }
}
