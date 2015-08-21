using IronVelocity.Compilation;
using IronVelocity.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests
{
    public abstract class OutputTestBase
    {

        protected void TestExpectedMarkupGenerated(string input, string expectedOutput, IDictionary<string, object> environment = null, string fileName = "", bool? isGlobalEnvironment = null)
        {
            expectedOutput = Utility.NormaliseLineEndings(expectedOutput);
            var globals = isGlobalEnvironment.GetValueOrDefault(Utility.DefaultToGlobals)
                ? environment
                : null;
            var generatedOutput = GetNormalisedOutput(input, environment, globals, fileName);

            Assert.That(generatedOutput, Is.EqualTo(expectedOutput));
        }

        protected string GetNormalisedOutput(string input, IDictionary<string, object> environment, string fileName = "")
        {
            IDictionary<string, object> globals = null;
            if (Utility.DefaultToGlobals)
            {
                globals = environment
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Key, x => x.Value);
            }

            return GetNormalisedOutput(input, environment, globals, fileName);

        }

        protected string GetNormalisedOutput(string input, IDictionary<string, object> environment, IDictionary<string, object> globals, string fileName = "")
        {
            var action = CompileTemplate(input, fileName, globals);

            var ctx = environment as VelocityContext;
            if (ctx == null)
                ctx = new VelocityContext(environment);

            using (var writer = new StringWriter())
            {
                var output = new VelocityOutput(writer);
                action(ctx, output);
                return Utility.NormaliseLineEndings(writer.ToString());
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

        protected VelocityTemplateMethod CompileTemplate(string input, string fileName = "", IDictionary<string, object> globals = null)
        {
            var parser = new IronVelocity.Parser.AntlrVelocityParser();
            var runtime = new VelocityRuntime(parser, globals);
            return runtime.CompileTemplate(input, Utility.GetName(), fileName, true);
        }
    }
}
