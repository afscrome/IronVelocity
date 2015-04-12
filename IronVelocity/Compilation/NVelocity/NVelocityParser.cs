using IronVelocity.Compilation.AST;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation
{
    public class NVelocityParser : IParser
    {
        internal readonly RuntimeInstance _runtimeService;

        private readonly IDictionary<string, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<string, DirectiveExpressionBuilder>(StringComparer.OrdinalIgnoreCase)
        {
            {"foreach", new ForeachDirectiveExpressionBuilder()},
            {"literal", new LiteralDirectiveExpressionBuilder()},
            {"macro", new MacroDefinitionExpressionBuilder()}
        };

        public NVelocityParser(IDictionary<string, DirectiveExpressionBuilder> directiveHandlers)
        {
            _runtimeService = new RuntimeInstance();

            var properties = new Commons.Collections.ExtendedProperties();
            properties.AddProperty("input.encoding", "utf-8");
            properties.AddProperty("output.encoding", "utf-8");
            properties.AddProperty("velocimacro.permissions.allow.inline", "true");
            properties.AddProperty("runtime.log.invalid.references", "false");
            properties.AddProperty("runtime.log.logsystem.class", typeof(TestLog).AssemblyQualifiedName.Replace(",", ";"));
            properties.AddProperty("parser.pool.size", 0);
            properties.AddProperty("velocimacro.permissions.allow.inline.local.scope", "true");
            ArrayList userDirectives = new ArrayList();

            if (directiveHandlers != null)
            {
                foreach (var directive in directiveHandlers)
                {
                    //userDirectives.Add(directive.Key.AssemblyQualifiedName);
                    _directiveHandlers.Add(directive);
                }
                properties.AddProperty("userdirective", userDirectives);
            }

            _runtimeService.Init(properties);
        }

        public Expression<VelocityTemplateMethod> Parse(string input, string name)
        {
            var log = TemplateGenerationEventSource.Log;
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                log.ParseStart(name);
                var ast = parser.Parse(reader, name) as ASTprocess;
                log.ParseStop(name);
                if (ast == null)
                    throw new InvalidProgramException("Unable to parse ast");

                var builder = new VelocityExpressionBuilder(_directiveHandlers);
                var nvelocityBuilder = new NVelocityExpressions(builder);
                log.ConvertToExpressionTreeStart(name);
                var expr = new RenderedBlock(nvelocityBuilder.GetBlockExpressions(ast), builder);
                log.ConvertToExpressionTreeStop(name);

                return Expression.Lambda<VelocityTemplateMethod>(expr, name, new[] { Constants.InputParameter, builder.OutputParameter });
            }
        }
    }
}
