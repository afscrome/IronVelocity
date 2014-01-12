using Commons.Collections;
using IronVelocity.Directives;
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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity
{
    public class VelocityRuntime
    {
        private RuntimeInstance _runtimeService;
        private IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<Type, DirectiveExpressionBuilder>()
        {
            {typeof(Foreach), new ForeachDirectiveExpressionBuilder()},
            {typeof(ForeachBeforeAllSection), new ForeachSectionExpressionBuilder(ForeachSection.BeforeAll)},
            {typeof(ForeachBeforeSection), new ForeachSectionExpressionBuilder(ForeachSection.Before)},
            {typeof(ForeachEachSection), new ForeachSectionExpressionBuilder(ForeachSection.Each)},
            {typeof(ForeachOddSection), new ForeachSectionExpressionBuilder(ForeachSection.Odd)},
            {typeof(ForeachEvenSection), new ForeachSectionExpressionBuilder(ForeachSection.Even)},
            {typeof(ForeachBetweenSection), new ForeachSectionExpressionBuilder(ForeachSection.Between)},
            {typeof(ForeachAfterSection), new ForeachSectionExpressionBuilder(ForeachSection.After)},
            {typeof(ForeachAfterAllSection), new ForeachSectionExpressionBuilder(ForeachSection.AfterAll)},
            {typeof(ForeachNoDataSection), new ForeachSectionExpressionBuilder(ForeachSection.NoData)},
        };

        ExtendedProperties _properties;
        public VelocityRuntime(IDictionary<Type, DirectiveExpressionBuilder> directiveHandlers)
        {
            _runtimeService = new RuntimeInstance();

            _properties = new Commons.Collections.ExtendedProperties();
            _properties.AddProperty("input.encoding", "utf-8");
            _properties.AddProperty("output.encoding", "utf-8");
            _properties.AddProperty("velocimacro.permissions.allow.inline", "false");
            _properties.AddProperty("runtime.log.invalid.references", "false");
            _properties.AddProperty("runtime.log.logsystem.class", typeof(NVelocity.Runtime.Log.NullLogSystem).AssemblyQualifiedName.Replace(",", ";"));

            ArrayList userDirectivces = new ArrayList();
            if (directiveHandlers != null)
            {
                foreach (var directive in directiveHandlers)
                {
                    userDirectivces.Add(directive.Key.AssemblyQualifiedName);
                    _directiveHandlers.Add(directive);
                }
                _properties.AddProperty("userdirective", userDirectivces);
            }

            _runtimeService.Init(_properties);
        }


        public Action<VelocityContext, StringBuilder> CompileTemplate(string input, string name)
        {
#if DEBUGD
            return CompileWithDebug(input, name);
#else
            var expressionTree = GetExpressionTree(input, name);
            return expressionTree.Compile();
#endif
        }

        private Expression<Action<VelocityContext, StringBuilder>> GetExpressionTree(string input, string name)
        {
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                var ast = parser.Parse(reader, null) as ASTprocess;
                if (ast == null)
                    throw new InvalidProgramException("Unable to parse ast");

                var converter = new VelocityASTConverter(_directiveHandlers);
                var expr = converter.BuildExpressionTree(ast);


                return Expression.Lambda<Action<VelocityContext, StringBuilder>>(expr, name, new[] { Constants.InputParameter, Constants.OutputParameter });
            }
        }


        public Action<VelocityContext, StringBuilder> CompileWithDebug(string input, string name)
        {
            AssemblyName assemblyName = new AssemblyName("Widgets");
            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, true);

            var debugAttributes =
                DebuggableAttribute.DebuggingModes.Default |
                DebuggableAttribute.DebuggingModes.DisableOptimizations;

            ConstructorInfo constructor =
                    typeof(DebuggableAttribute).GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
            var cab = new CustomAttributeBuilder(constructor, new object[] { debugAttributes });
            assemblyBuilder.SetCustomAttribute(cab);
            moduleBuilder.SetCustomAttribute(cab);

             TypeBuilder typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);


            MethodBuilder meth = typeBuilder.DefineMethod(
                    "Execute",
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(void),
                    new Type[] { typeof(VelocityContext), typeof(StringBuilder) });

            var expressionTree = GetExpressionTree(input, name);
            var debugInfo = DebugInfoGenerator.CreatePdbGenerator();
            expressionTree.CompileToMethod(meth, debugInfo);

            return (VelocityContext ctx, StringBuilder builder) => { meth.Invoke(null, new object[] { ctx, builder }); };

        }
    }
}
