using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace IronVelocity.Compilation
{
    public delegate void VelocityTemplateMethod(VelocityContext context, StringBuilder builder);
    public class VelocityCompiler
    {
        private const string _methodName = "Execute";
        private static readonly Type[] _signature = new[] { typeof(VelocityContext), typeof(StringBuilder) };

        private readonly IReadOnlyDictionary<string, Type> _globals;

        public VelocityCompiler(IDictionary<string, Type> globals)
        {
            if (globals != null)
            {
                _globals = new Dictionary<string, Type>(globals, StringComparer.OrdinalIgnoreCase);
            }
        }


        public VelocityTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, bool debugMode, string fileName)
        {
            var assemblyName = new AssemblyName("Widgets");
            //RunAndCollect allows this assembly to be garbage collected when finished with - http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

            return CompileWithSymbols(expressionTree, name, assemblyBuilder, debugMode, fileName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="Final cast will fail if the Expression does not conform to VelocityTemplateMethod's signature")]
        public VelocityTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode, string fileName)
        {
            if (assemblyBuilder == null)
                throw new ArgumentNullException("assemblyBuilder");

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(name, true);

            if (debugMode)
            {
                AddDebugAttributes(assemblyBuilder, moduleBuilder);
            }

            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);

            var meth = typeBuilder.DefineMethod(
                    _methodName,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(void),
                    _signature);

            if (_globals != null && _globals.Count > 0)
            {
                var staticTypeVisitor = new StaticGlobalVisitor(_globals);
                expressionTree = (Expression<VelocityTemplateMethod>)staticTypeVisitor.Visit(expressionTree);
            }

            var debugVisitor = new DynamicToExplicitCallSiteConvertor(typeBuilder, fileName);
            expressionTree = (Expression<VelocityTemplateMethod>)debugVisitor.Visit(expressionTree);

            var debugInfo = DebugInfoGenerator.CreatePdbGenerator();
            expressionTree.CompileToMethod(meth, debugInfo);


            var compiledType = typeBuilder.CreateType();
            var compiledMethod = compiledType.GetMethod(_methodName, _signature);
            return (VelocityTemplateMethod)Delegate.CreateDelegate(typeof(VelocityTemplateMethod), compiledMethod);
        }

        public static void AddDebugAttributes(AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder)
        {
            if (assemblyBuilder == null)
                throw new ArgumentNullException("assemblyBuilder");
            if (moduleBuilder == null)
                throw new ArgumentNullException("moduleBuilder");

            var debugAttributes =
                DebuggableAttribute.DebuggingModes.Default |
                DebuggableAttribute.DebuggingModes.DisableOptimizations;

            var constructor = typeof(DebuggableAttribute).GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
            var cab = new CustomAttributeBuilder(constructor, new object[] { debugAttributes });
            assemblyBuilder.SetCustomAttribute(cab);
            moduleBuilder.SetCustomAttribute(cab);
        }


    }
}
