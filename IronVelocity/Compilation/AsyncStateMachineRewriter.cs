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

        private static readonly MethodInfo _setExceptionMethodInfo = typeof(AsyncTaskMethodBuilder).GetMethod("SetException", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Exception) }, null);
        private static readonly MethodInfo _setResultMethodInfo = typeof(AsyncTaskMethodBuilder).GetMethod("SetResult", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);


        private AsyncStateMachineRewriter() { }

        public static Expression<T> ConvertToAsyncStateMachine<T>(Expression expression, params ParameterExpression[] args)
        {
            var rewriter = new AsyncStateMachineRewriter();

            var mainBody = rewriter.Visit(expression);

            var returnLabel = Expression.Label("return");

            var setCompleteState = Expression.Assign(rewriter.AsyncState, Expression.Constant(-2));

            var catchBlock = Expression.Catch(
                rewriter.CaughtException,
                Expression.Block(
                    setCompleteState,
                    Expression.Call(rewriter.AsyncTaskMethodBuilder, _setExceptionMethodInfo, rewriter.CaughtException),
                    Expression.Return(returnLabel)
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
                    Expression.Call(rewriter.AsyncTaskMethodBuilder, _setResultMethodInfo),
                    Expression.Label(returnLabel)
                ),
            args
           );
    }

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
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            throw new NotSupportedException();
        }
        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            throw new NotSupportedException();
        }

    }



}
