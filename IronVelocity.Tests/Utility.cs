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
        public static VelocityTemplateMethod CompileTemplate(string input, string fileName = "", IDictionary<string, object> globals = null)
        {
            //VelocityAsyncTemplateMethod action = null;
            VelocityTemplateMethod action = null;
            try
            {
                var runtime = new VelocityRuntime(null, globals);
                return runtime.CompileTemplate(input, "TestExpression", fileName, true);
            }
            catch (NotSupportedException ex)
            {
                //Temporary for dev to separate those tests failing due to errors from those due to not being implemented yet
                Assert.Inconclusive(ex.Message);
                throw new NotImplementedException();
            }
        }

        public static IDictionary<string, object> Evaluate(string input, IDictionary<string, object> environment, string fileName = "", IDictionary<string, object> globals = null)
        {
            if (globals == null && environment != null)
                globals = environment.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);

            var action = CompileTemplate(input, fileName, globals);

            var builder = new StringBuilder();
            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);
            
            action(ctx, builder);

            return ctx;
        }

        public static String GetNormalisedOutput(string input, IDictionary<string, object> environment, string fileName = "", IDictionary<string, object> globals = null)
        {
            var action = CompileTemplate(input, fileName, globals);

            var builder = new StringBuilder();
            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            /*var task = action(ctx, builder);
            task.Wait();

            if (task.IsFaulted)
                throw task.Exception;
            if (task.Status != TaskStatus.RanToCompletion)
                throw new InvalidOperationException();
            */
            action(ctx, builder);

            return NormaliseLineEndings(builder.ToString());
        }

        public static void TestExpectedMarkupGenerated(string input, string expectedOutput, IDictionary<string, object> environment = null, string fileName = "", bool isGlobalEnvironment = true)
        {
            expectedOutput = NormaliseLineEndings(expectedOutput);
            var globals = isGlobalEnvironment ? environment : null;
            var generatedOutput = GetNormalisedOutput(input, environment, fileName, globals);

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
