using IronVelocity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public static class Utility
    {
        public static Action<VelocityContext, StringBuilder> BuildGenerator(string input, IDictionary<string, object> environment = null)
        {
            var symbolGenerator = System.Runtime.CompilerServices.DebugInfoGenerator.CreatePdbGenerator();
            var tree = VelocityExpressionTreeBuilder.BuildExpressionTree(input, environment);
            
            //TODO: Look into Compile to Method to support PDB info
            return tree.Compile();
        }

        public static String GetNormalisedOutput(string input, IDictionary<string, object> environment)
        {
            Action<VelocityContext, StringBuilder> action = null;
            try
            {
                action = BuildGenerator(input, environment);
            }
            catch (NotSupportedException ex)
            {
                //Temporary for dev to seperte those tests failing due to errors from those due to not being implemented yet
                Assert.Inconclusive(ex.Message);
            }

            var builder = new StringBuilder();
            var ctx = new VelocityContext(environment);
            action(ctx, builder);

            return NormaliseLineEndings(builder.ToString());
        }

        public static void TestExpectedMarkupGenerated(string input, string expectedOutput, IDictionary<string, object> environment = null)
        {
            expectedOutput = NormaliseLineEndings(expectedOutput);
            var generatedOutput = GetNormalisedOutput(input, environment);

            Assert.AreEqual(expectedOutput, generatedOutput);
        }

        /// <summary>
        /// Normalises line endings for the current platform
        /// </summary>
        /// <param name="text">The text to normalise line endings in</param>
        /// <returns>the input text with '\r\n' (windows), '\r' (mac) and '\n' (*nix) replaced by Environment.NewLine</returns>
        public static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }
    }
}
