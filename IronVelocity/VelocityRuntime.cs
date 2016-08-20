using IronVelocity.Binders;
using IronVelocity.Compilation;
using IronVelocity.Directives;
using IronVelocity.Parser;
using System.Collections.Immutable;
using System.IO;

namespace IronVelocity
{
	public class VelocityRuntime
    {
		private static IImmutableList<CustomDirectiveBuilder> DefaultDirectives { get; } = ImmutableList.Create<CustomDirectiveBuilder>(new ForeachDirectiveBuilder());
		public IImmutableDictionary<string, object> Globals { get; }
		public IImmutableList<CustomDirectiveBuilder> Directives { get; }
		public bool ReduceWhitesapce { get; }
		public bool OptimizeConstantTypes { get; }

		private readonly IParser _parser;
        private readonly VelocityCompiler _compiler;

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

			_parser = new AntlrVelocityParser(Directives, expressionFactorry, options.ReduceWhitespace);

			_compiler = new VelocityCompiler(debugMode: options.CompileInDebugMode);
        }


        public VelocityTemplateMethod CompileTemplate(TextReader input, string typeName, string fileName)
        {
            var template = _parser.Parse(input, typeName);
            return _compiler.CompileWithSymbols(template, typeName, fileName);
        }
    }
}
