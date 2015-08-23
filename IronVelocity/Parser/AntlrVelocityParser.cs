using System.Linq.Expressions;
using IronVelocity.Compilation;
using Antlr4.Runtime;

namespace IronVelocity.Parser
{
    public class AntlrVelocityParser : IParser
    {
        public Expression<VelocityTemplateMethod> Parse(string input, string name)
        {
            var log = TemplateGenerationEventSource.Log;

            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VelocityParser(tokenStream)
            {
                ErrorHandler = new BailErrorStrategy()
            };

            log.ParseStart(name);
            var template = parser.template();
            log.ParseStop(name);

            //TODO: Log parse tree to ETW - update "log.LogParsedExpressionTree();" to use antlr parse tree.

            var visitor = new AntlrToExpressionTreeCompiler();

            log.ConvertToExpressionTreeStart(name);
            var body = visitor.Visit(template);
            log.ConvertToExpressionTreeStop(name);

            return Expression.Lambda<VelocityTemplateMethod>(body, name, new[] { Constants.InputParameter, Constants.OutputParameter });
        }
    }
}
