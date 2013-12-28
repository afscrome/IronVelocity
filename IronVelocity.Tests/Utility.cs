using IronVelocity;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public static class Utility
    {
        public static Action<StringBuilder> BuildGenerator(string input, IDictionary<string, object> environment = null)
        {
            return VelocityExpressionTreeBuilder.BuildExpressionTree(input, environment).Compile();
        }

        public static String GetNormalisedOutput(string input, IDictionary<string, object> environment)
        {
            Action<StringBuilder> action = null;
            try
            {
                action = BuildGenerator(input, environment);
            }
            catch (NotSupportedException ex)
            {
                Assert.Inconclusive(ex.Message);
            }

            var builder = new StringBuilder();
            action(builder);

            return NormaliseLineEndings(builder.ToString());
        }

        public static void TestExpectedMarkupGenerated(string input, string expectedOutput, IDictionary<string, object> environment = null)
        {
            expectedOutput = NormaliseLineEndings(expectedOutput);
            var generatedOutput = GetNormalisedOutput(input, environment);

            Assert.AreEqual(expectedOutput, generatedOutput);
        }

        public static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        }
    }
}
