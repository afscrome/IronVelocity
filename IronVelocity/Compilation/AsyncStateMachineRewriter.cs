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
        private readonly ParameterExpression CaughtException = Expression.Parameter(typeof(Exception), "exception");
        private readonly LabelTarget ReturnTarget = Expression.Label("return");
        private readonly List<LabelTarget> AsyncReturnTargets = new List<LabelTarget>(); 

        private static readonly MethodInfo _setExceptionMethodInfo = typeof(AsyncTaskMethodBuilder).GetMethod("SetException", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Exception) }, null);
        private static readonly MethodInfo _setResultMethodInfo = typeof(AsyncTaskMethodBuilder).GetMethod("SetResult", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
        private static readonly PropertyInfo _isComplete = typeof(Task).GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance, null, typeof(bool), new Type[0], null);

        private AsyncStateMachineRewriter() { }

        public static Expression<T> ConvertToAsyncStateMachine<T>(Expression expression, params ParameterExpression[] args)
        {
            return new AsyncStateMachineRewriter().MakeStateMachineMethod<T>(expression, args);
        }

        private Expression<T> MakeStateMachineMethod<T>(Expression body, params ParameterExpression[] args)
        {
            var mainBody = Visit(body);

            var setCompleteState = Expression.Assign(AsyncState, Expression.Constant(-2));

            var catchBlock = Expression.Catch(
                CaughtException,
                Expression.Block(
                    setCompleteState,
                    Expression.Call(AsyncTaskMethodBuilder, _setExceptionMethodInfo, CaughtException),
                    Expression.Return(ReturnTarget)
                )
            );

            List<SwitchCase> cases = new List<System.Linq.Expressions.SwitchCase>();

            return Expression.Lambda<T>(
                Expression.Block(
                    Expression.TryCatch(
                        Expression.Block(mainBody),
                        catchBlock
                    ),
                    setCompleteState,
                    Expression.Call(AsyncTaskMethodBuilder, _setResultMethodInfo),
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
                var taskLocal = Expression.Parameter(node.Type);
                return Expression.Block(
                    new[] { taskLocal },
                    Expression.Assign(taskLocal, base.VisitMethodCall(node)),
                    Expression.IfThenElse(
                        Expression.Property(taskLocal, _isComplete),
                        Constants.EmptyExpression,
                        Expression.Block(
                            Expression.Throw(Expression.New(typeof(NotImplementedException)))
                            //awaiter = task.GetAwaiter()
                            //Set State Expression.Assign(AsyncState, Expression.Constant(1234)),
                            //awaiter.UnsafeOnCompleted(MoveNext);
                            //builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                            //return; Expression.Return(ReturnTarget)
                        )
                    ),
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
