using IronVelocity.Compilation;
using IronVelocity.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IronVelocity.Tests.TemplateExecution
{
    public abstract class TemplateExeuctionBase
    {
        public class ExecutionResult
        {
            public string Output { get; }
            public IReadOnlyDictionary<string, object> Context { get; set; }

            public ExecutionResult(StringBuilder output, IReadOnlyDictionary<string,object> context)
            {
                Output = output.ToString();
                Context = context;
            }
        }

        public object EvaluateExpression(string input, IDictionary<string, object> locals = null)
        {
            input = $"#set($result = {input})";
            var result = ExecuteTemplate(input, locals);
            Assert.That(result.Output, Is.Empty);
            return result.Context["result"];
        }

        public ExecutionResult ExecuteTemplate(string input, IDictionary<string,object> locals = null)
        {
            var template = CompileTemplate(input, fileName: Utility.GetName());

            var context = new VelocityContext(locals);

            var outputBuilder = new StringBuilder();
            using (var outputWriter = new StringWriter(outputBuilder))
            {
                var output = new VelocityOutput(outputWriter);
                template(context, output);
            }

            return new ExecutionResult(outputBuilder, context);
        }

        private VelocityTemplateMethod CompileTemplate(string input, string fileName = "", IDictionary<string, object> globals = null)
        {
            var parser = new IronVelocity.Parser.AntlrVelocityParser();
            var runtime = new VelocityRuntime(parser, globals);
            return runtime.CompileTemplate(input, Utility.GetName(), fileName, true);
        }
    }
}
