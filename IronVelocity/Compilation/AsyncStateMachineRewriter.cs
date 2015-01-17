using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation
{
    public class AsyncStateMachineRewriter : ExpressionVisitor
    {
        private readonly ParameterExpression AsyncTaskMethodBuilder = Constants.AsyncTaskMethodBuilderParameter;
        private readonly ParameterExpression AsyncState = Constants.AsyncStateParameter;
        private readonly ParameterExpression StateMachine = Constants.StateMachineParameter;

        private readonly ParameterExpression CaughtException = Expression.Parameter(typeof(Exception), "exception");
        private readonly LabelTarget ReturnTarget = Expression.Label("return");
        private readonly List<LabelTarget> ContinuationTargets = new List<LabelTarget>(); 

        private static readonly MethodInfo _setExceptionMethodInfo = typeof(AsyncTaskMethodBuilder).GetMethod("SetException", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Exception) }, null);
        private static readonly PropertyInfo _isComplete = typeof(Task).GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance, null, typeof(bool), new Type[0], null);
        private static readonly MethodInfo _configureAwaiter = typeof(VelocityAsyncCompiler.VelocityAsyncStateMachine).GetMethod("ConfigureAwaiter");

        private AsyncStateMachineRewriter() { }

        public static Expression<T> ConvertToAsyncStateMachine<T>(Expression expression, params ParameterExpression[] args)
        {
            return new AsyncStateMachineRewriter().MakeStateMachineMethod<T>(expression, args);
        }

        private Expression SetStateExpression(int value)
        {
            return Expression.Assign(AsyncState, Expression.Constant(value));
        }

        private Expression<T> MakeStateMachineMethod<T>(Expression body, params ParameterExpression[] args)
        {
            var mainBody = Visit(body);

            List<SwitchCase> cases = new List<System.Linq.Expressions.SwitchCase>();

            for (int i = 0; i < ContinuationTargets.Count; i++)
            {
                cases.Add(Expression.SwitchCase(Expression.Goto(ContinuationTargets[i]), Expression.Constant(i)));                
            }
            //cases.Add(Expression.SwitchCase(Expression.Throw(Expression.New(typeof(NotImplementedException))), Expression.Constant(2)));

            return Expression.Lambda<T>(
                Expression.Block(
                    Expression.Switch(AsyncState, cases.ToArray()),
                    mainBody,
                    SetStateExpression(-2),
                    Expression.Label(ReturnTarget)
                ),
                args
            );

        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //Definite Task
            if (node.Type == typeof(Task))
            {
                int state = ContinuationTargets.Count + 1;
                var continuationTarget = Expression.Label("Continuation" + state);
                ContinuationTargets.Add(continuationTarget);

                var taskLocal = Expression.Parameter(node.Type, "task" + state);
                return Expression.Block( Expression.Block(
                    new[] { taskLocal },
                    Expression.IfThen(
                        Expression.Not(Expression.Property(Expression.Assign(taskLocal, base.VisitMethodCall(node)), _isComplete)),
                        Expression.Block(
                            SetStateExpression(state),
                            Expression.Call(StateMachine, _configureAwaiter, taskLocal),
                            Expression.Return(ReturnTarget)
                        )
                    )),
                    Expression.Label(continuationTarget),
                    Constants.VoidReturnValue
                );
            }
            //Definite Task<T>
            if (typeof(Task).IsAssignableFrom(node.Type))
            {
                throw new NotImplementedException("TODO: Definite Task<T> support");
            }
            //Possible Task
            else if (node.Type.IsAssignableFrom(typeof(Task)))
            {
                throw new NotImplementedException("TODO: Possible Task support");
            }
            //Not a Task
            else
            {
                return base.VisitMethodCall(node);
            } 
        }


        protected override Expression VisitDynamic(DynamicExpression node)
        {
            if (typeof(Task).IsAssignableFrom(node.Type))
            {
                throw new NotImplementedException("TODO: Definite Task support");
            }
            //Possible Task
            else if (node.Type.IsAssignableFrom(typeof(Task)))
            {
                throw new NotImplementedException("TODO: Possible Task support");
            }
            //Not a Task
            else
            {
                return base.VisitDynamic(node);
            }
        }

        #region Control Flow Expressions
        protected override Expression VisitTry(TryExpression node)
        {
            throw new NotSupportedException();
        }
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            throw new NotSupportedException();
        }
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            throw new NotSupportedException();
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            throw new NotSupportedException();
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            //TODO: Will need to support for foreach
            throw new NotImplementedException();
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            return base.VisitLabel(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            throw new NotImplementedException();
        }

        #endregion

    }



}
