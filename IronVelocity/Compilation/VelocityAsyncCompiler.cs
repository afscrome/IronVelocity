using System;
using System.Collections.Generic;
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


    public class VelocityAsyncCompiler
    {
        private const string _methodName = "Execute";
        private static readonly Type[] _signature = new[] {
            typeof (VelocityAsyncStateMachine).MakeByRefType(),
            typeof(int).MakeByRefType(),
            typeof(VelocityContext),
            typeof(StringBuilder)
        };
        internal delegate void VelocityAsyncTemplateMethodInternal(ref VelocityAsyncStateMachine stateMachine, ref int state, VelocityContext context, StringBuilder builder);

        private readonly IReadOnlyDictionary<string, Type> _globals;

        public VelocityAsyncCompiler(IDictionary<string, Type> globals)
        {
            if (globals != null)
            {
                _globals = new Dictionary<string, Type>(globals, StringComparer.OrdinalIgnoreCase);
            }
        }


        public VelocityAsyncTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, bool debugMode, string fileName)
        {
            var assemblyName = new AssemblyName("Widgets");
            //RunAndCollect allows this assembly to be garbage collected when finished with - http://msdn.microsoft.com/en-us/library/dd554932(VS.100).aspx
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            //TODO: use debug mode from web.config rather than debugger attached
            return CompileWithSymbols(expressionTree, name, assemblyBuilder, debugMode, fileName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Final cast will fail if the Expression does not conform to VelocityAsyncTemplateMethod's signature")]
        public VelocityAsyncTemplateMethod CompileWithSymbols(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode, string fileName)
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

                stateMachine.Builder.Start(ref stateMachine);

                return stateMachine.Builder.Task;
            });

        }

        private VelocityAsyncTemplateMethodInternal CompileStateMachineMoveNext(Expression<VelocityTemplateMethod> expressionTree, string name, AssemblyBuilder assemblyBuilder, bool debugMode, string fileName)
        {
            if (assemblyBuilder == null)
                throw new ArgumentNullException("assemblyBuilder");

            var args = new ParameterExpression[] {
                Constants.StateMachineParameter,
                Constants.AsyncStateParameter,
                expressionTree.Parameters[0],
                expressionTree.Parameters[1]
            };


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

            if (_globals != null && _globals.Count > 0)
            {
                var staticTypeVisitor = new StaticGlobalVisitor(_globals);
                expressionTree = staticTypeVisitor.VisitAndConvert(expressionTree, "Static Conversion");
            }

            var asyncExpressionTree = AsyncStateMachineRewriter.ConvertToAsyncStateMachine<VelocityAsyncTemplateMethodInternal>(expressionTree.Body, args); 
            
            var reducer = new DynamicToExplicitCallSiteConvertor(typeBuilder, fileName);
            asyncExpressionTree = reducer.VisitAndConvert(asyncExpressionTree, "Debug Reducer");

            var debugInfo = DebugInfoGenerator.CreatePdbGenerator();
            asyncExpressionTree.CompileToMethod(meth, debugInfo);

            var compiledType = typeBuilder.CreateType();
            var compiledMethod = compiledType.GetMethod(meth.Name, _signature);
            return (VelocityAsyncTemplateMethodInternal)Delegate.CreateDelegate(typeof(VelocityAsyncTemplateMethodInternal), compiledMethod);
        }


        [CompilerGenerated]
        public struct VelocityAsyncStateMachine : IAsyncStateMachine
        {
            internal VelocityAsyncTemplateMethodInternal TemplateMethod;
            internal VelocityContext Context;
            internal StringBuilder Output;
            private int State;
            internal AsyncTaskMethodBuilder Builder;
            private TaskAwaiter _awaiter;


            public void MoveNext()
            {
                try
                {
                    TemplateMethod(ref this, ref State, Context, Output);
                    if (State == -2)
                        Builder.SetResult();
                }
                catch (Exception ex)
                {
                    State = -1;
                    Builder.SetException(ex);
                }
            }

            [DebuggerHidden]
            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                Builder.SetStateMachine(stateMachine);
            }

            public void ConfigureAwaiter(ref Task task)
            {
                _awaiter = task.GetAwaiter();
                _awaiter.UnsafeOnCompleted(MoveNext);
                Builder.AwaitUnsafeOnCompleted(ref _awaiter, ref this);
            }
        }
    }
}
