using IronVelocity.Compilation;
using IronVelocity.Directives;
using IronVelocity.Parser;
using IronVelocity.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IronVelocity.Tests.TemplateExecution
{
    public abstract class TemplateExeuctionBase
    {
        protected readonly IReadOnlyCollection<CustomDirectiveBuilder> DefaultCustomDirectives = new[] { new ForeachDirectiveBuilder() };

        public class ExecutionResult
        {
            public string Output { get; }
            public string OutputWithNormalisedLineEndings => Utility.NormaliseLineEndings(Output);
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

        public ExecutionResult ExecuteTemplate(string input, IDictionary<string,object> locals = null, IReadOnlyCollection<CustomDirectiveBuilder> customDirectives = null)
        {
            var template = CompileTemplate(input, Utility.GetName(), null, customDirectives);

            var context = new VelocityContext(locals);

            var outputBuilder = new StringBuilder();
            using (var outputWriter = new StringWriter(outputBuilder))
            {
                var output = new VelocityOutput(outputWriter);
                template(context, output);
            }

            return new ExecutionResult(outputBuilder, context);
        }

        private VelocityTemplateMethod CompileTemplate(string input, string fileName, IDictionary<string, object> globals, IReadOnlyCollection<CustomDirectiveBuilder> customDirectives)
        {
            var parser = new AntlrVelocityParser(customDirectives);
            var runtime = new VelocityRuntime(parser, globals);
            return runtime.CompileTemplate(input, Utility.GetName(), fileName, true);
        }
    }
}
