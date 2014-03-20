using Commons.Collections;
using IronVelocity.Compilation;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Linq;
using NVelocity.Runtime.Resource;

namespace IronVelocity
{
    public class VelocityRuntime
    {
        internal readonly RuntimeInstance _runtimeService;
        private readonly IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<Type, DirectiveExpressionBuilder>()
        {
            {typeof(Foreach), new ForEachDirectiveExpressionBuilder()},
            {typeof(ForeachBeforeAllSection), new ForEachSectionExpressionBuilder(ForEachSection.BeforeAll)},
            {typeof(ForeachBeforeSection), new ForEachSectionExpressionBuilder(ForEachSection.Before)},
            {typeof(ForeachEachSection), new ForEachSectionExpressionBuilder(ForEachSection.Each)},
            {typeof(ForeachOddSection), new ForEachSectionExpressionBuilder(ForEachSection.Odd)},
            {typeof(ForeachEvenSection), new ForEachSectionExpressionBuilder(ForEachSection.Even)},
            {typeof(ForeachBetweenSection), new ForEachSectionExpressionBuilder(ForEachSection.Between)},
            {typeof(ForeachAfterSection), new ForEachSectionExpressionBuilder(ForEachSection.After)},
            {typeof(ForeachAfterAllSection), new ForEachSectionExpressionBuilder(ForEachSection.AfterAll)},
            {typeof(ForeachNoDataSection), new ForEachSectionExpressionBuilder(ForEachSection.NoData)},
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

        private Expression<VelocityTemplateMethod> GetExpressionTree(string input, string typeName, string fileName)
        {
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                var ast = parser.Parse(reader, null) as ASTprocess;
                if (ast == null)
                    throw new InvalidProgramException("Unable to parse ast");

                var converter = new VelocityASTConverter(_directiveHandlers);
                var expr = converter.BuildExpressionTree(ast, fileName);

                return Expression.Lambda<VelocityTemplateMethod>(expr, typeName, new[] { Constants.InputParameter, Constants.OutputParameter });
            }
        }


        public VelocityTemplateMethod CompileTemplate(string input, string typeName, string fileName, bool debugMode)
        {
            var tree = GetExpressionTree(input, typeName, fileName);
            return VelocityCompiler.CompileWithSymbols(tree, typeName, debugMode);
        }

    }
}
