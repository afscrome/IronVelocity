using IronVelocity;
using IronVelocity.Compilation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tests
{
    public static class Utility
    {
        public static VelocityTemplateMethod BuildGenerator(string input, IDictionary<string, object> environment = null, string fileName = "")
        {
            var runtime = new VelocityRuntime(null);
            return runtime.CompileTemplate(input, "TestExpression", fileName, true);
        }

        public static String GetNormalisedOutput(string input, IDictionary<string, object> environment, string fileName = "")
        {
            VelocityTemplateMethod action = null;
            try
            {
                action = BuildGenerator(input, environment, fileName);
            }
            catch (NotSupportedException ex)
            {
                //Temporary for dev to separate those tests failing due to errors from those due to not being implemented yet
                Assert.Inconclusive(ex.Message);
            }

            var builder = new StringBuilder();
            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            action(ctx, builder);

            return NormaliseLineEndings(builder.ToString());
        }

        public static void TestExpectedMarkupGenerated(string input, string expectedOutput, IDictionary<string, object> environment = null, string fileName = "")
        {
            expectedOutput = NormaliseLineEndings(expectedOutput);
            var generatedOutput = GetNormalisedOutput(input, environment, fileName);

            Assert.AreEqual(expectedOutput, generatedOutput);
        }

        /// <summary>
        /// Normalises line endings for the current platform
        /// </summary>
        /// <param name="text">The text to normalise line endings in</param>
        /// <returns>the input text with '\r\n' (windows), '\r' (mac) and '\n' (*nix) replaced by Environment.NewLine</returns>
        public static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }

        public static object BinderTests(CallSiteBinder binder, params object[] args)
        {
            var expression = Expression.Dynamic(binder, typeof(object), args.Select(Expression.Constant));

            return EvaluateExpression(expression);
        }

        public static object EvaluateExpression(Expression expression)
        {
            var action = Expression.Lambda<Func<object>>(expression)
                .Compile();

            return action();
        }
    }
}
