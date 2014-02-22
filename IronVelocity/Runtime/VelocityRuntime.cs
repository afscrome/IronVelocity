using Commons.Collections;
using IronVelocity.Compilation;
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
            _properties.AddProperty("parser.pool.size", 0);
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


        public Action<VelocityContext, StringBuilder> CompileTemplate(string input, string typeName, string fileName)
        {
            return CompileWithSymbols(input, typeName, fileName);
            //TODO: More detailed investigation of DynamicAssembly vs. DynamicMethod
            /*
            var expressionTree = GetExpressionTree(input, typeName, fileName);
            return expressionTree.Compile();
             */
        }

        private Expression<Action<VelocityContext, StringBuilder>> GetExpressionTree(string input, string typeName, string fileName)
        {
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                var ast = parser.Parse(reader, null) as ASTprocess;
                if (ast == null)
                    throw new InvalidProgramException("Unable to parse ast");

                var converter = new VelocityASTConverter(_directiveHandlers);
                var expr = converter.BuildExpressionTree(ast, fileName);


                return Expression.Lambda<Action<VelocityContext, StringBuilder>>(expr, typeName, new[] { Constants.InputParameter, Constants.OutputParameter });
            }
        }

        private const string _methodName = "Execute";
        private static readonly Type[] _signature = new[] { typeof(VelocityContext), typeof(StringBuilder) };
        public Action<VelocityContext, StringBuilder> CompileWithSymbols(string input, string name, string fileName)
        {
            var assemblyName = new AssemblyName("Widgets");
            //RunAndCollect allows this assembly to be garbage collected when finished with - http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            return CompileWithSymbols(input, name, fileName, assemblyBuilder);
        }

        public Action<VelocityContext, StringBuilder> CompileWithSymbols(string input, string name, string fileName, AssemblyBuilder assemblyBuilder, bool debugMode = false)
        {
            if (assemblyBuilder == null)
                throw new ArgumentNullException("assemblyBuilder");

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(name, true);

            if (debugMode)
            {
                var debugAttributes =
                    DebuggableAttribute.DebuggingModes.Default |
                    DebuggableAttribute.DebuggingModes.DisableOptimizations;

                var constructor = typeof(DebuggableAttribute).GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
                var cab = new CustomAttributeBuilder(constructor, new object[] { debugAttributes });
                assemblyBuilder.SetCustomAttribute(cab);
                moduleBuilder.SetCustomAttribute(cab);
            }

            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);

            var meth = typeBuilder.DefineMethod(
                    _methodName,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(void),
                    _signature);


            var expressionTree = GetExpressionTree(input, name, fileName);

            var reducer = new DynamicToExplicitCallSiteConvertor(typeBuilder);

            expressionTree = (Expression<Action<VelocityContext, StringBuilder>>)reducer.Visit(expressionTree);

            var debugInfo = DebugInfoGenerator.CreatePdbGenerator();
            expressionTree.CompileToMethod(meth, debugInfo);

            var compiledType = typeBuilder.CreateType();
            var compiledMethod = compiledType.GetMethod(_methodName, _signature);
            return (Action<VelocityContext, StringBuilder>)Delegate.CreateDelegate(typeof(Action<VelocityContext, StringBuilder>), compiledMethod);


        }
    }
}
