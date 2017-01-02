using System.Linq.Expressions;
using IronVelocity.Compilation;
using Antlr4.Runtime;
using System;
using System.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Atn;
using System.Collections.Generic;
using IronVelocity.Directives;
using System.IO;
using System.Collections.Immutable;

namespace IronVelocity.Parser
{
    public class AntlrVelocityParser : IParser
    {
		private readonly IImmutableList<CustomDirectiveBuilder> _customDirectives;
		private readonly VelocityExpressionFactory _expressionFactory;
		private readonly bool _reduceWhitespace;

        public AntlrVelocityParser(IImmutableList<CustomDirectiveBuilder> customDirectives, VelocityExpressionFactory expressionFactory, bool reduceWhitespace = false)
        {
			if (expressionFactory == null)
				throw new ArgumentNullException(nameof(expressionFactory));

			_customDirectives = customDirectives ?? ImmutableList<CustomDirectiveBuilder>.Empty;
            _expressionFactory = expressionFactory;
			_reduceWhitespace = reduceWhitespace;
		}

		public Expression<VelocityTemplateMethod> Parse(TextReader input, string name)
        {
            var charStream = new AntlrInputStream(input);

            var template = ParseTemplate(charStream, name, x => x.template());
            return CompileToTemplateMethod(template, name);
        }


        internal T ParseTemplate<T>(ICharStream input, string name, Func<VelocityParser, T> parseFunc, int? lexerMode = null)
            where T : RuleContext
        {
            var lexerErrorListener = new ErrorListener<int>();
            var parserErrorListener = new ErrorListener<IToken>();

            var lexer = new VelocityLexer(input);
            var tokenStream = new CommonTokenStream(lexer);
			var parser = new VelocityParser(tokenStream) {
				DirectiveBuilders = _customDirectives,
			};

            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(lexerErrorListener);

            parser.RemoveErrorListeners();
            //parser.AddErrorListener(new DiagnosticErrorListener(false));
            parser.AddErrorListener(parserErrorListener);

            var originalErrorStrategy = parser.ErrorHandler;
            parser.ErrorHandler = new BailErrorStrategy();

            if (lexerMode.HasValue)
                lexer.Mode(lexerMode.Value);

            parser.Interpreter.PredictionMode = PredictionMode.Sll;

            T template;
            try {
                TemplateGenerationEventSource.Log.ParseStart(name);
                template = parseFunc(parser);
                TemplateGenerationEventSource.Log.ParseStop(name);
            }
            catch(Exception)
            {
                tokenStream.Reset();
                parser.Reset();
                parser.Interpreter.PredictionMode = PredictionMode.Ll;
                parser.ErrorHandler = originalErrorStrategy;

                template = parseFunc(parser);
                Console.WriteLine("Fell back to full LL parsing");
            }

            HandleFailures("Lexer error", lexerErrorListener);
            HandleFailures("Parser error", parserErrorListener);

            if (lexer.Token.Type != -1)
            {
#if DEBUG
                foreach(var token in lexer.GetAllTokens())
                {
                    Console.WriteLine(token);
                }
#endif
                throw new ParseCanceledException("Lexer failed to lex to end of file.");
            }

            if (parser.NumberOfSyntaxErrors > 0)
                throw new ParseCanceledException("Parser syntax errors occurred, but weren't reported properly");

            return template;
        }


        internal Expression<VelocityTemplateMethod> CompileToTemplateMethod(RuleContext parsed, string name)
        {
            var visitor = new AntlrToExpressionTreeCompiler(this, _customDirectives, _expressionFactory, _reduceWhitespace);

            TemplateGenerationEventSource.Log.ConvertToExpressionTreeStart(name);
            var body = visitor.Visit(parsed);
            TemplateGenerationEventSource.Log.ConvertToExpressionTreeStop(name);

            return Expression.Lambda<VelocityTemplateMethod>(body, name, new[] { Constants.InputParameter, Constants.OutputParameter });
        }

        private void HandleFailures<T>(string prefix, ErrorListener<T> errorListener)
        {
            if (errorListener.Errors?.Any() ?? false)
            {
                var message = string.Join(", \r\n", errorListener.Errors.Select(x => $"Line: {x.Line} Position {x.Chartacter}: {x.Message}"));
                throw new ParseCanceledException($"{prefix}: {message}");
            }

        }

    }
}
