using IronVelocity.Compilation;
using System;
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

namespace IronVelocity.Compilation
{
    public static class VelocityCompiler
    {
        private const string _methodName = "Execute";
        private static readonly Type[] _signature = new[] { typeof(VelocityContext), typeof(StringBuilder) };

        public static Action<VelocityContext, StringBuilder> CompileWithSymbols(Expression<Action<VelocityContext, StringBuilder>> expressionTree, string name)
        {
            var assemblyName = new AssemblyName("Widgets");
            //RunAndCollect allows this assembly to be garbage collected when finished with - http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var debug = System.Diagnostics.Debugger.IsAttached;
            return CompileWithSymbols(expressionTree, name, assemblyBuilder, debug);
        }

        public static Action<VelocityContext, StringBuilder> CompileWithSymbols(Expression<Action<VelocityContext, StringBuilder>> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode)
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
