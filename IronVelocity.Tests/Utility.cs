using IronVelocity;
using NUnit.Framework;
using System.Text;

namespace Tests
{
    public static class Utility
    {
        public static void TestExpectedMarkupGenerated(string input, string expectedOutput)
        {
            var action = VelocityExpressionTreeBuilder.BuildExpressionTree(input).Compile();

            var builder = new StringBuilder();
            action(builder);

            Assert.AreEqual(NormaliseLineEndings(expectedOutput), NormaliseLineEndings(builder.ToString()));
        }

        public static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
