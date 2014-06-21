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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Final cast will fail if the Expression does not conform to VelocityAsyncTemplateMethod's signature")]
        public static VelocityAsyncTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode, string fileName)
        {

            var syncMethod = VelocityCompiler.CompileWithSymbols(expressionTree, name, assemblyBuilder, debugMode, fileName);


            return (VelocityAsyncTemplateMethod)((VelocityContext context, StringBuilder output) =>
            {
                var stateMachine = new VelocityAsyncStateMachine()
                {
                    Method = syncMethod,
                    Context = context,
                    Output = output
                };

                stateMachine.AsyncMethodBuilder.Start(ref stateMachine);

                return stateMachine.AsyncMethodBuilder.Task;
            });

        }

        internal static Type BuildStateMachine(TypeBuilder typeBuilder)
        {

            var asyncTaskMethodBuilder = typeBuilder.DefineField("AsyncTaskMethodBuilder", typeof(AsyncTaskMethodBuilder), FieldAttributes.Public);

            BuildSetStateMachineMethod(typeBuilder, asyncTaskMethodBuilder);
            BuildMoveNextMethod(typeBuilder);

            typeBuilder.AddInterfaceImplementation(typeof(IAsyncStateMachine));

            var compiledType = typeBuilder.CreateType();

            return compiledType;   
        }

        internal static void BuildSetStateMachineMethod(TypeBuilder typeBuilder, FieldInfo stateMachineField)
        {
            var setStateMachineMethodBuilder = typeBuilder.DefineMethod("SetStateMachine",
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new[] { typeof(IAsyncStateMachine) }
            );

            var debuggerHiddenAttributeBuilder = new CustomAttributeBuilder(MethodHelpers.DebuggerHiddenConstructorInfo, new object[0]);
            setStateMachineMethodBuilder.SetCustomAttribute(debuggerHiddenAttributeBuilder);

            var ilGen = setStateMachineMethodBuilder.GetILGenerator();
            //Emit: AsyncMethodBuilder.SetStateMachine(stateMachine);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldflda, stateMachineField);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.EmitCall(OpCodes.Call, MethodHelpers.SetAsyncMethodBuilderStateMachine, null);
            ilGen.Emit(OpCodes.Ret);            
        }

        internal static void BuildMoveNextMethod(TypeBuilder typeBuilder)
        {
            var moveNextMethodBuilder = typeBuilder.DefineMethod("MoveNext",
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                Type.EmptyTypes
            );

            var ilGen = moveNextMethodBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Ret);
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
