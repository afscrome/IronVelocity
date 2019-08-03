using IronVelocity.Binders;
using IronVelocity.Compilation;
using IronVelocity.Directives;
using IronVelocity.Parser;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

namespace IronVelocity
{
	public class VelocityRuntime
    {
		private static IImmutableList<CustomDirectiveBuilder> DefaultDirectives { get; } = ImmutableList.Create<CustomDirectiveBuilder>(new ForeachDirectiveBuilder());
		public IImmutableDictionary<string, object> Globals { get; }
		public IImmutableList<CustomDirectiveBuilder> Directives { get; }
		public bool ReduceWhitesapce { get; }
		public bool OptimizeConstantTypes { get; }
        public bool Debug { get; }

		private readonly IParser _parser;
#if NETFRAMEWORK
        private readonly VelocityCompiler _compiler;
#endif

        public VelocityRuntime(VelocityRuntimeOptions options)
        {
			Directives = options.Directives?.ToImmutableArray() ?? DefaultDirectives;
			Globals = options.Globals?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty;
			ReduceWhitesapce = options.ReduceWhitespace;
			OptimizeConstantTypes = true;

			var binderFactory = options.BinderFactory ?? new ReusableBinderFactory(new BinderFactory());
			var expressionFactorry = OptimizeConstantTypes
				? new ConstantTypeOptimisingVelocityExpressionFactory(binderFactory, Globals)
				: new VelocityExpressionFactory(binderFactory);

            Debug = options.CompileInDebugMode;
			_parser = new AntlrVelocityParser(Directives, expressionFactorry, options.ReduceWhitespace);

#if NETFRAMEWORK
            _compiler = new VelocityCompiler(debugMode: options.CompileInDebugMode);
#endif
        }


        public VelocityTemplateMethod CompileTemplate(TextReader input, string typeName, string fileName)
        {
            var template = _parser.Parse(input, typeName);
#if NETFRAMEWORK
            return _compiler.CompileWithSymbols(template, typeName, fileName);
#else
            var debugInfoGenerator = Debug ? DebugInfoGenerator.CreatePdbGenerator() : null;
            return template.Compile(debugInfoGenerator);
#endif
        }
    }
}
