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

namespace IronVelocity.Parser
{
    public class AntlrVelocityParser : IParser
    {
        public static IReadOnlyCollection<CustomDirectiveBuilder> DefaultDirectives { get; } = new[]
        {
            new ForeachDirectiveBuilder()
        };

        private readonly IReadOnlyCollection<CustomDirectiveBuilder> _customDirectives;

        public AntlrVelocityParser()
            : this(null)
        {
        }

        public AntlrVelocityParser(IReadOnlyCollection<CustomDirectiveBuilder> customDirectives)
        {
            _customDirectives = customDirectives ?? DefaultDirectives;
        }

        public Expression<VelocityTemplateMethod> Parse(string input, string name)
        {
            var charStream = new AntlrInputStream(input);

            var template = ParseTemplate(charStream, name, x => x.template());
            return CompileToTemplateMethod(template, name);
        }

        public Expression<VelocityTemplateMethod> Parse(Stream input, string name)
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
            var parser = new VelocityParser(tokenStream);

            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(lexerErrorListener);

            parser.RemoveErrorListeners();
            //parser.AddErrorListener(new DiagnosticErrorListener(false));
            parser.AddErrorListener(parserErrorListener);

            var originalErrorStrategy = parser.ErrorHandler;
            parser.ErrorHandler = new BailErrorStrategy();
            parser.BlockDirectives = _customDirectives.Where(x => x.IsBlockDirective).Select(x => x.Name).ToList();

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
                //throw new Exception("TODO: Log that we've needed to fallback to full LL parsing.  If this happens a lot, may want to fallback to single phase parsing");
            }

            HandleFailures("Lexer error", lexerErrorListener);
            HandleFailures("Parser error", parserErrorListener);

            if (lexer.Token.Type != -1)
                throw new ParseCanceledException("Lexer failed to lex to end of file.");

            if (parser.NumberOfSyntaxErrors > 0)
                throw new ParseCanceledException("Parser syntax errors occurred, but weren't reported properly");

            return template;
        }


        internal Expression<VelocityTemplateMethod> CompileToTemplateMethod(RuleContext parsed, string name)
        {
            var visitor = new AntlrToExpressionTreeCompiler(this, _customDirectives);

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
