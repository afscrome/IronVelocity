using IronVelocity.Compilation;
using IronVelocity.Compilation.AST;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace IronVelocity
{
    public class VelocityRuntime
    {
        internal readonly RuntimeInstance _runtimeService;
        private static readonly IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<Type, DirectiveExpressionBuilder>()
        {
            {typeof(Foreach), new ForeachDirectiveExpressionBuilder()},
            {typeof(Literal), new LiteralDirectiveExpressionBuilder()},
        };

        public VelocityRuntime(IDictionary<Type, DirectiveExpressionBuilder> directiveHandlers)
        {
            _runtimeService = new RuntimeInstance();

            var properties = new Commons.Collections.ExtendedProperties();
            properties.AddProperty("input.encoding", "utf-8");
            properties.AddProperty("output.encoding", "utf-8");
            properties.AddProperty("velocimacro.permissions.allow.inline", "false");
            properties.AddProperty("runtime.log.invalid.references", "false");
            properties.AddProperty("runtime.log.logsystem.class", typeof(NVelocity.Runtime.Log.NullLogSystem).AssemblyQualifiedName.Replace(",", ";"));
            properties.AddProperty("parser.pool.size", 0);
            ArrayList userDirectives = new ArrayList();
            if (directiveHandlers != null)
            {
                foreach (var directive in directiveHandlers)
                {
                    userDirectives.Add(directive.Key.AssemblyQualifiedName);
                    _directiveHandlers.Add(directive);
                }
                properties.AddProperty("userdirective", userDirectives);
            }
            _runtimeService.Init(properties);
        }

        public VelocityTemplateMethod CompileTemplate(string input, string typeName, string fileName, bool debugMode)
        {
            var tree = GetExpressionTree(input, typeName);
            return VelocityCompiler.CompileWithSymbols(tree, typeName, debugMode, fileName);
        }

        public VelocityAsyncTemplateMethod CompileAsyncTemplate(string input, string typeName, string fileName, bool debugMode)
        {
            var tree = GetExpressionTree(input, typeName);
            return VelocityAsyncCompiler.CompileWithSymbols(tree, typeName, debugMode, fileName);
        }

        private Expression<VelocityTemplateMethod> GetExpressionTree(string input, string typeName)
        {
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                var ast = parser.Parse(reader, null) as ASTprocess;
                if (ast == null)
                    throw new InvalidProgramException("Unable to parse ast");

                var builder = new VelocityExpressionBuilder(_directiveHandlers);
                var expr = new RenderedBlock(ast, builder);

                return Expression.Lambda<VelocityTemplateMethod>(expr, typeName, new[] { Constants.InputParameter, builder.OutputParameter });
            }
        }
    }
}
