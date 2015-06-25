using IronVelocity.Compilation.AST;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime;
using NVelocity.Runtime.Log;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace IronVelocity.Compilation
{
    public class NVelocityParser : IParser
    {
        internal readonly RuntimeInstance _runtimeService;

        private readonly IDictionary<string, object> _globals;
        private readonly IDictionary<string, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<string, DirectiveExpressionBuilder>(StringComparer.OrdinalIgnoreCase)
        {
            {"foreach", new ForeachDirectiveExpressionBuilder()},
            {"literal", new LiteralDirectiveExpressionBuilder()},
            {"macro", new MacroDefinitionExpressionBuilder()}
        };

        public NVelocityParser(IDictionary<string, DirectiveExpressionBuilder> directiveHandlers, IDictionary<string,object> globals)
        {
            _runtimeService = new RuntimeInstance();

            var properties = new Commons.Collections.ExtendedProperties();
            properties.AddProperty("input.encoding", "utf-8");
            properties.AddProperty("output.encoding", "utf-8");
            properties.AddProperty("velocimacro.permissions.allow.inline", "true");
            properties.AddProperty("runtime.log.invalid.references", "false");
            properties.AddProperty("runtime.log.logsystem.class", typeof(NullLogSystem).AssemblyQualifiedName.Replace(",", ";"));
            properties.AddProperty("parser.pool.size", 0);
            properties.AddProperty("velocimacro.permissions.allow.inline.local.scope", "true");
            ArrayList userDirectives = new ArrayList();

            if (directiveHandlers != null)
            {
                foreach (var directive in directiveHandlers)
                {
                    userDirectives.Add(directive.Value.NVelocityDirectiveType.AssemblyQualifiedName);
                    _directiveHandlers.Add(directive);
                }
                properties.AddProperty("userdirective", userDirectives);
            }

            _runtimeService.Init(properties);
            _globals = globals;
        }
        public Expression<VelocityTemplateMethod> Parse(string input, string name)
        {
            using (var reader = new StringReader(input))
            {
                return Parse(reader, name);
            }
        }

        public Expression<VelocityTemplateMethod> Parse(TextReader reader, string name)
        {
            var parser = _runtimeService.CreateNewParser();
            var log = TemplateGenerationEventSource.Log;

            log.ParseStart(name);
            var ast = parser.Parse(reader, name) as ASTprocess;
            log.ParseStop(name);
            if (ast == null)
                throw new InvalidProgramException("Unable to parse ast");

            var builder = new VelocityExpressionBuilder(_directiveHandlers);
            var converter = new NVelocityNodeToExpressionConverter(builder, _runtimeService, name, _globals);

            log.ConvertToExpressionTreeStart(name);
            var expr = new RenderedBlock(converter.GetBlockExpressions(ast));
            log.ConvertToExpressionTreeStop(name);

            return Expression.Lambda<VelocityTemplateMethod>(expr, name, new[] { Constants.InputParameter, Constants.OutputParameter });
            
        }

    }
}
