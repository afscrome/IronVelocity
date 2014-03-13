using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace IronVelocity.Compilation
{
    public delegate void VelocityTemplateMethod(VelocityContext context, StringBuilder builder);
    public static class VelocityCompiler
    {
        private const string _methodName = "Execute";
        private static readonly Type[] _signature = new[] { typeof(VelocityContext), typeof(StringBuilder) };


        public static VelocityTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name)
        {
            var assemblyName = new AssemblyName("Widgets");
            //RunAndCollect allows this assembly to be garbage collected when finished with - http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            //TODO: use debug mode from web.config rather than debugger attached
            var debug = System.Diagnostics.Debugger.IsAttached;
            return CompileWithSymbols(expressionTree, name, assemblyBuilder, debug);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="Final cast will fail if the Expression does not conform to VelocityTemplateMethod's signature")]
        public static VelocityTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode)
        {
            if (assemblyBuilder == null)
                throw new ArgumentNullException("assemblyBuilder");

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(name, true);

            if (true)
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


            var reducer = new DynamicToExplicitCallSiteConvertor(typeBuilder);

            expressionTree = (Expression<VelocityTemplateMethod>)reducer.Visit(expressionTree);

            var debugInfo = DebugInfoGenerator.CreatePdbGenerator();
            expressionTree.CompileToMethod(meth, debugInfo);

            var compiledType = typeBuilder.CreateType();
            var compiledMethod = compiledType.GetMethod(_methodName, _signature);
            return (VelocityTemplateMethod)Delegate.CreateDelegate(typeof(VelocityTemplateMethod), compiledMethod);
        }

    }
}
