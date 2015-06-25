﻿using IronVelocity.Compilation;
using IronVelocity.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace IronVelocity.Tests
{
    public static class Utility
    {
        public static bool DefaultToGlobals = true;

        private static string GetName()
        {
            var testContext = NUnit.Framework.TestContext.CurrentContext;
            return testContext == null
                ? "TestExpression"
                : testContext.Test.Name;
        }

        public static VelocityTemplateMethod CompileTemplate(string input, string fileName = "", IDictionary<string, object> globals = null)
        {
            var parser = new NVelocityParser(null, globals);
            var runtime = new VelocityRuntime(parser, globals);
            return runtime.CompileTemplate(input, GetName(), fileName, true);
        }

        public static IDictionary<string, object> Evaluate(string input, IDictionary<string, object> environment, string fileName = "", IDictionary<string, object> globals = null)
        {
            if (DefaultToGlobals && globals == null && environment != null)
                globals = environment.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);

            var action = CompileTemplate(input, fileName, globals);

            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            using (var writer = new StringWriter())
            {
                var output = new VelocityOutput(writer);
                action(ctx, output);
            }

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

            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            using (var writer = new StringWriter())
            {
                var output = new VelocityOutput(writer);
                action(ctx, output);
                return NormaliseLineEndings(writer.ToString());
            }
            
            /*
            var task = action(ctx, builder);
            task.Wait();

            if (task.IsFaulted)
                throw task.Exception;
            if (task.Status != TaskStatus.RanToCompletion)
                throw new InvalidOperationException();
            */

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
