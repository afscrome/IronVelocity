using IronVelocity.Compilation;
using IronVelocity.Compilation.AST;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity
{
    public class TestLog : NVelocity.Runtime.Log.ILogSystem
    {
        public void Init(IRuntimeServices rs)
        {
        }

        public void LogVelocityMessage(NVelocity.Runtime.Log.LogLevel level, string message)
        {
            Console.WriteLine("[{0}] message");
        }
    }


    public class VelocityRuntime
    {
        internal readonly RuntimeInstance _runtimeService;
        private readonly VelocityCompiler _compiler;
        private readonly VelocityAsyncCompiler _asyncCompiler;
        private readonly IReadOnlyDictionary<string, object> _globals;

        private readonly IDictionary<string, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<string, DirectiveExpressionBuilder>(StringComparer.OrdinalIgnoreCase)
        {
            {"foreach", new ForeachDirectiveExpressionBuilder()},
            {"literal", new LiteralDirectiveExpressionBuilder()},
            {"macro", new MacroDefinitionExpressionBuilder()}
        };

        public VelocityRuntime(IDictionary<string, DirectiveExpressionBuilder> directiveHandlers, IDictionary<string, object> globals)
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

            if (globals == null)
                _globals = new Dictionary<string, object>();
            else
                _globals = new Dictionary<string, object>(globals, StringComparer.OrdinalIgnoreCase);

            var globalsTypeMap = _globals.ToDictionary(x => x.Key, x => x.Value.GetType());
            _compiler = new VelocityCompiler(globalsTypeMap);
            _asyncCompiler = new VelocityAsyncCompiler(globalsTypeMap);

        }


        public VelocityTemplateMethod CompileTemplate(string input, string typeName, string fileName, bool debugMode)
        {
            var tree = GetExpressionTree(input, typeName);
            return _compiler.CompileWithSymbols(tree, typeName, debugMode, fileName);
        }

        public VelocityAsyncTemplateMethod CompileAsyncTemplate(string input, string typeName, string fileName, bool debugMode)
        {
            var tree = GetExpressionTree(input, typeName);
            return _asyncCompiler.CompileWithSymbols(tree, typeName, debugMode, fileName);
        }

        internal Expression<VelocityTemplateMethod> GetExpressionTree(string input, string typeName)
        {
            var log = TemplateGenerationEventSource.Log;
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                log.ParseStart(typeName);
                var ast = parser.Parse(reader, typeName) as ASTprocess;
                log.ParseStop(typeName);
                if (ast == null)
                    throw new InvalidProgramException("Unable to parse ast");

                var builder = new VelocityExpressionBuilder(_directiveHandlers);
                log.ConvertToExpressionTreeStart(typeName);
                var expr = new RenderedBlock(builder.GetBlockExpressions(ast), builder);
                log.ConvertToExpressionTreeStop(typeName);

                return Expression.Lambda<VelocityTemplateMethod>(expr, typeName, new[] { Constants.InputParameter, builder.OutputParameter });
            }
        }
    }
}
