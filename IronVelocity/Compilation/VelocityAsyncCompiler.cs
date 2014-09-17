using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation
{
    public delegate Task VelocityAsyncTemplateMethod(VelocityContext context, StringBuilder builder);


    public static class VelocityAsyncCompiler
    {
        private const string _methodName = "Execute";
        private static readonly Type[] _signature = new[] {typeof(AsyncTaskMethodBuilder), typeof(int).MakeByRefType(), typeof(VelocityContext), typeof(StringBuilder) };
        private delegate void VelocityAsyncTemplateMethodInternal(AsyncTaskMethodBuilder asyncTaskMethodBuilder, ref int state, VelocityContext context, StringBuilder builder);


        public static VelocityAsyncTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, bool debugMode, string fileName)
        {
            var assemblyName = new AssemblyName("Widgets");
            //RunAndCollect allows this assembly to be garbage collected when finished with - http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            //TODO: use debug mode from web.config rather than debugger attached
            return CompileWithSymbols(expressionTree, name, assemblyBuilder, debugMode, fileName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Final cast will fail if the Expression does not conform to VelocityAsyncTemplateMethod's signature")]
        public static VelocityAsyncTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode, string fileName)
        {
            var syncMethod = CompileStateMachineMoveNext(expressionTree, name, assemblyBuilder, debugMode, fileName);

            return (VelocityAsyncTemplateMethod)((VelocityContext context, StringBuilder output) =>
            {
                var stateMachine = new VelocityAsyncStateMachine()
                {
                    TemplateMethod = syncMethod,
                    Context = context,
                    Output = output
                };

                stateMachine.AsyncMethodBuilder.Start(ref stateMachine);

                return stateMachine.AsyncMethodBuilder.Task;
            });

        }

        private static VelocityAsyncTemplateMethodInternal CompileStateMachineMoveNext(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode, string fileName)
        {
            if (assemblyBuilder == null)
                throw new ArgumentNullException("assemblyBuilder");

            var args = new ParameterExpression[] {Constants.AsyncTaskMethodBuilderParameter, Constants.AsyncStateParameter, expressionTree.Parameters[0], expressionTree.Parameters[1] };
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(name, true);

            if (debugMode)
            {
                VelocityCompiler.AddDebugAttributes(assemblyBuilder, moduleBuilder);
            }

            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);

            var meth = typeBuilder.DefineMethod(
                    _methodName + "Async",
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(void),
                    _signature);


            var reducer = new DynamicToExplicitCallSiteConvertor(typeBuilder, fileName);

            expressionTree = (Expression<VelocityTemplateMethod>)reducer.Visit(expressionTree);
            var asyncExpressionTree = Expression.Lambda<VelocityAsyncTemplateMethodInternal>(expressionTree.Body, args);

            var debugInfo = DebugInfoGenerator.CreatePdbGenerator();
            asyncExpressionTree.CompileToMethod(meth, debugInfo);

            var compiledType = typeBuilder.CreateType();
            var compiledMethod = compiledType.GetMethod(meth.Name, _signature);
            return (VelocityAsyncTemplateMethodInternal)Delegate.CreateDelegate(typeof(VelocityAsyncTemplateMethodInternal), compiledMethod);


        }


        [CompilerGenerated]
        private struct VelocityAsyncStateMachine : IAsyncStateMachine
        {
            public VelocityAsyncTemplateMethodInternal TemplateMethod;
            public VelocityContext Context;
            public StringBuilder Output;
            public int State;
            public AsyncTaskMethodBuilder AsyncMethodBuilder;


            public void MoveNext()
            {
                try
                {
                    TemplateMethod(AsyncMethodBuilder, ref State, Context, Output);
                }
                catch (Exception ex)
                {
                    AsyncMethodBuilder.SetException(ex);
                    return;
                }
                this.AsyncMethodBuilder.SetResult();
            }

            [DebuggerHidden]
            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                AsyncMethodBuilder.SetStateMachine(stateMachine);
            }
        }
    }
}
