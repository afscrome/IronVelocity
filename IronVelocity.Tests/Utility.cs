using IronVelocity;
using IronVelocity.Compilation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public static class Utility
    {
        private static bool DefaultToGlobals = false;

        private static string GetName()
        {
            var testContext = NUnit.Framework.TestContext.CurrentContext;
            return testContext == null
                ? "TestExpression"
                : testContext.Test.Name;
        }

        public static VelocityAsyncTemplateMethod CompileAsyncTemplate(string input, string fileName = "", IDictionary<string, object> globals = null)
        {
            var runtime = new VelocityRuntime(null, globals);
            throw new NotImplementedException();
            //return runtime.CompileAsyncTemplate(input, GetName(), fileName, true);
        }

        public static VelocityTemplateMethod CompileTemplate(string input, string fileName = "", IDictionary<string, object> globals = null)
        {
            var parser = new NVelocityParser(null);
            var runtime = new VelocityRuntime(parser, globals);
            return runtime.CompileTemplate(input, GetName(), fileName, true);
        }

        public static IDictionary<string, object> Evaluate(string input, IDictionary<string, object> environment, string fileName = "", IDictionary<string, object> globals = null)
        {
            if (DefaultToGlobals && globals == null && environment != null)
                globals = environment.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);

            var action = CompileTemplate(input, fileName, globals);

            var builder = new StringBuilder();
            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            action(ctx, builder);

            return ctx;
        }
        public static String GetNormalisedOutput(string input, IDictionary<string, object> environment, string fileName = "")
        {
            IDictionary<string,object> globals = null;
            if (DefaultToGlobals)
            {
                globals = environment
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Key, x => x.Value);
            }

            return GetNormalisedOutput(input, environment, globals, fileName);

        }

        public static String GetNormalisedOutput(string input, IDictionary<string, object> environment, IDictionary<string, object> globals, string fileName = "")
        {
            var action = CompileTemplate(input, fileName, globals);

            var builder = new StringBuilder();
            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            action(ctx,builder);
            
            /*
            var task = action(ctx, builder);
            task.Wait();

            if (task.IsFaulted)
                throw task.Exception;
            if (task.Status != TaskStatus.RanToCompletion)
                throw new InvalidOperationException();
            */

            return NormaliseLineEndings(builder.ToString());
        }


        public static void TestExpectedMarkupGenerated(string input, string expectedOutput, IDictionary<string, object> environment = null, string fileName = "", bool? isGlobalEnvironment = null)
        {
            expectedOutput = NormaliseLineEndings(expectedOutput);
            var globals = isGlobalEnvironment.GetValueOrDefault(DefaultToGlobals)
                ? environment
                : null;
            var generatedOutput = GetNormalisedOutput(input, environment, globals, fileName);

            Assert.AreEqual(expectedOutput, generatedOutput);
        }


        public static async Task<string> GetNormalisedOutputAsync(string input, IDictionary<string, object> environment, string fileName = "", IDictionary<string, object> globals = null)
        {
            var action = CompileAsyncTemplate(input, fileName, globals);

            var builder = new StringBuilder();
            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            await action(ctx, builder);

            return NormaliseLineEndings(builder.ToString());
        }

        public static async Task TestExpectedMarkupGeneratedAsync(string input, string expectedOutput, IDictionary<string, object> environment = null, string fileName = "", bool isGlobalEnvironment = true)
        {
            expectedOutput = NormaliseLineEndings(expectedOutput);
            var globals = isGlobalEnvironment ? environment : null;
            var generatedOutput = await GetNormalisedOutputAsync(input, environment, fileName, globals);

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
            return BinderTests<object>(binder, args);
        }

        public static T BinderTests<T>(CallSiteBinder binder, params object[] args)
        {
            var expression = Expression.Dynamic(binder, typeof(T), args.Select(Expression.Constant));

            var action = Expression.Lambda<Func<T>>(expression)
                .Compile();

            return action();
        }
    }
}
