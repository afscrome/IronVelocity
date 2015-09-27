﻿using System.Linq.Expressions;
using IronVelocity.Compilation;
using Antlr4.Runtime;
using System;
using System.Linq;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Atn;
using System.Collections.Generic;
using IronVelocity.Directives;

namespace IronVelocity.Parser
{
    public class AntlrVelocityParser : IParser
    {
        private readonly IReadOnlyCollection<CustomDirectiveBuilder> _customDirectives;

        public AntlrVelocityParser(IReadOnlyCollection<CustomDirectiveBuilder> customDirectives)
        {
            _customDirectives = customDirectives ?? new CustomDirectiveBuilder[0];
        }

        public Expression<VelocityTemplateMethod> Parse(string input, string name)
        {
            var template = ParseTemplate(input, name, x=> x.template());
            return CompileToTemplateMethod(template, name);
        }



        internal T ParseTemplate<T>(string input, string name, Func<VelocityParser, T> parseFunc, int? lexerMode = null)
            where T : RuleContext
        {
            var lexerErrorListener = new ErrorListener<int>();
            var parserErrorListener = new ErrorListener<IToken>();

            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VelocityParser(tokenStream);

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
            var visitor = new AntlrToExpressionTreeCompiler(_customDirectives);

            TemplateGenerationEventSource.Log.ConvertToExpressionTreeStart(name);
            var body = visitor.Visit(parsed);
            TemplateGenerationEventSource.Log.ConvertToExpressionTreeStop(name);

            return Expression.Lambda<VelocityTemplateMethod>(body, name, new[] { Constants.InputParameter, Constants.OutputParameter });
        }

        private void HandleFailures<T>(string prefix, ErrorListener<T> errorListener)
        {
            if (errorListener.Errors?.Any() ?? false)
            {
                var message = String.Join(", \r\n", errorListener.Errors.Select(x => $"Line: {x.Line} Position {x.Chartacter}: {x.Message}"));
                throw new ParseCanceledException($"{prefix}: {message}");
            }

        }

    }
}
