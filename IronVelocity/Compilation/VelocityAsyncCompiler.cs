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
        private static readonly Type[] _signature = new[] { typeof(VelocityContext), typeof(StringBuilder) };


        public static VelocityAsyncTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, bool debugMode, string fileName)
        {
            var assemblyName = new AssemblyName("Widgets");
            //RunAndCollect allows this assembly to be garbage collected when finished with - http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            //TODO: use debug mode from web.config rather than debugger attached
            return CompileWithSymbols(expressionTree, name, assemblyBuilder, debugMode, fileName);
        }

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="Final cast will fail if the Expression does not conform to VelocityAsyncTemplateMethod's signature")]
        public static VelocityAsyncTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode, string fileName)
        {

            var syncMethod = VelocityCompiler.CompileWithSymbols(expressionTree, name, assemblyBuilder, debugMode, fileName);


            return (VelocityAsyncTemplateMethod)((VelocityContext context, StringBuilder output) => {
                var stateMachine = new VelocityAsyncStateMachine(){
                    Method = syncMethod,
                    Context = context,
                    Output = output
                };

                stateMachine.AsyncMethodBuilder.Start(ref stateMachine);

                return stateMachine.AsyncMethodBuilder.Task;
            });

        }


        [CompilerGenerated]
        private struct VelocityAsyncStateMachine : IAsyncStateMachine
        {
            public VelocityTemplateMethod Method;
            public VelocityContext Context;
            public StringBuilder Output;
            public AsyncTaskMethodBuilder AsyncMethodBuilder;

            public void MoveNext()
            {
                try
                {
                    Method(Context, Output);
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
