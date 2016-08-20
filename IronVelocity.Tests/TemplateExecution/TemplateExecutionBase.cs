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
using IronVelocity.Binders;
using System.Collections.Immutable;

namespace IronVelocity.Tests.TemplateExecution
{
    public enum StaticTypingMode
    {
        PromoteContextToGlobals,
        AsProvided
    }

    public abstract class TemplateExeuctionBase
    {
        protected TemplateExeuctionBase(StaticTypingMode mode)
        {
            StaticTypingMode = mode;
        }

        protected readonly IReadOnlyCollection<CustomDirectiveBuilder> DefaultCustomDirectives = new[] { new ForeachDirectiveBuilder() };

        protected virtual StaticTypingMode StaticTypingMode { get; }

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
                return dictionary.ToImmutableDictionary();

            var type = obj.GetType();
            if (type.Namespace != null)
                throw new ArgumentOutOfRangeException(nameof(obj), $"Type {type.FullName} is not an anonymous type");

            return type
                .GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(obj, null));
        }

        public object EvaluateExpression(string input, object locals = null)
        {
            input = $"#set($result = {input})";
            var result = ExecuteTemplate(input, locals);
            Assert.That(result.Output, Is.Empty);
            return result.Context["result"];
        }


        public ExecutionResult ExecuteTemplate(string input, object locals = null, object globals = null, IList<CustomDirectiveBuilder> customDirectives = null, string fileName = null, bool reduceWhitespace = false)
        {
            var localsDictionary = ConvertToDictionary(locals);
            var globalsDictionary = ConvertToDictionary(globals)?.ToImmutableDictionary();

            if (StaticTypingMode == StaticTypingMode.PromoteContextToGlobals && globals == null)
                globalsDictionary = localsDictionary?.Where(x => x.Value != null).ToImmutableDictionary(x => x.Key, x => x.Value);

            fileName = fileName ?? Utility.GetName();

            var template = CompileTemplate(input, fileName, globalsDictionary, customDirectives, reduceWhitespace);

            var context = new VelocityContext(localsDictionary);

            var outputBuilder = new StringBuilder();
            using (var outputWriter = new StringWriter(outputBuilder))
            {
                var output = new VelocityOutput(outputWriter);
                template(context, output);
            }

            return new ExecutionResult(outputBuilder, context);
        }

		protected virtual IBinderFactory CreateBinderFactory() => null;

		private VelocityTemplateMethod CompileTemplate(string input, string fileName, IDictionary<string, object> globals, IList<CustomDirectiveBuilder> directives, bool reduceWhitespace)
		{
			var options = new VelocityRuntimeOptions
			{
				Globals = globals,
				Directives = directives,
				ReduceWhitespace = reduceWhitespace,
				BinderFactory = CreateBinderFactory(),
				OptimizeConstantTypes = StaticTypingMode == StaticTypingMode.PromoteContextToGlobals
			};

			var runtime = new VelocityRuntime(options);
			using (var reader = new StringReader(input))
			{
				var template = runtime.CompileTemplate(reader, Utility.GetName(), fileName);
				return template;
			}
		}
    }
}
