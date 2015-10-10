using IronVelocity.Compilation;
using IronVelocity.Directives;
using IronVelocity.Parser;
using IronVelocity.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;

namespace IronVelocity.Tests.TemplateExecution
{
    public enum GlobalMode
    {
        Force,
        AsProvided
    }

    public abstract class TemplateExeuctionBase
    {
        protected TemplateExeuctionBase(GlobalMode mode)
        {
            GlobalMode = mode;
        }

        protected readonly IReadOnlyCollection<CustomDirectiveBuilder> DefaultCustomDirectives = new[] { new ForeachDirectiveBuilder() };

        protected virtual GlobalMode GlobalMode { get; }

        public class ExecutionResult
        {
            public string Output { get; }
            public string OutputWithNormalisedLineEndings => Utility.NormaliseLineEndings(Output);
            public IReadOnlyDictionary<string, object> Context { get; set; }

            public ExecutionResult(StringBuilder output, IReadOnlyDictionary<string, object> context)
            {
                Output = output.ToString();
                Context = context;
            }
        }

        private IDictionary<string, object> ConvertToDictionary(object obj)
        {
            if (obj == null)
                return null;

            var dictionary = obj as IDictionary<string, object>;
            if (dictionary != null)
                return dictionary;

            var type = obj.GetType();
            if (type.Namespace != null)
                throw new ArgumentOutOfRangeException(nameof(obj), $"Type {type.FullName} is not an anonymous type");

            return type
                .GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(obj, null));
        }

        public object EvaluateExpression(string input, IDictionary<string, object> locals = null)
        {
            input = $"#set($result = {input})";
            var result = ExecuteTemplate(input, locals);
            Assert.That(result.Output, Is.Empty);
            return result.Context["result"];
        }


        public ExecutionResult ExecuteTemplate(string input, object locals = null, object globals = null, IReadOnlyCollection<CustomDirectiveBuilder> customDirectives = null, string fileName = null)
        {
            var localsDictionary = ConvertToDictionary(locals);
            var globalsDictionary = ConvertToDictionary(globals);

            if (GlobalMode == GlobalMode.Force && (!globalsDictionary?.Any() ?? true))
                globalsDictionary = localsDictionary?.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);

            fileName = fileName ?? Utility.GetName();
            var globals2 = globalsDictionary != null
                ? new Dictionary<string, object>(globalsDictionary)
                : null;

            var template = CompileTemplate(input, fileName, globals2, customDirectives);

            var context = new VelocityContext(localsDictionary);

            var outputBuilder = new StringBuilder();
            using (var outputWriter = new StringWriter(outputBuilder))
            {
                var output = new VelocityOutput(outputWriter);
                template(context, output);
            }

            return new ExecutionResult(outputBuilder, context);
        }


        private VelocityTemplateMethod CompileTemplate(string input, string fileName, IReadOnlyDictionary<string, object> globals, IReadOnlyCollection<CustomDirectiveBuilder> customDirectives)
        {
            //This is for debugging - change it with the Immediate window if you need to dump a test to disk for further investigation.
            bool saveDllAndExtractIlForTroubleshooting = false;

            var parser = new AntlrVelocityParser(customDirectives, globals);

            VelocityDiskCompiler diskCompiler = null;

            var runtime = new VelocityRuntime(parser);
            if (saveDllAndExtractIlForTroubleshooting)
            {
                var assemblyName = Path.GetFileName(fileName);
                diskCompiler = new VelocityDiskCompiler(new AssemblyName(assemblyName), ".");
                new VelocityRuntime(parser, diskCompiler);
            }

            var template = runtime.CompileTemplate(input, Utility.GetName(), fileName, true);

            if (saveDllAndExtractIlForTroubleshooting)
            {
                diskCompiler.SaveDll();
                diskCompiler.SaveIl();
            }
            return template;
        }
    }
}
