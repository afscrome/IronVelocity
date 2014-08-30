using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.Async
{
    [TestFixture]
    [Ignore("Async work in progress")]
    public class GeneratedTypeTests
    {
        [Test]
        public void AsyncTask_Produces_No_Output()
        {
            var input = "$x.AsyncTask()";
            var expected = "";

            var asyncObj = new AsyncObj();
            var context = new Dictionary<string, object>{
                { "x", asyncObj}
            };

            Utility.TestExpectedMarkupGenerated(input, expected, context);
            Assert.True(asyncObj.HasRun);
        }


        [Test]
        public void AsyncTask_IsContinued()
        {
            var input = "Before $x.AsyncTask() After";
            var expected = "Before  After";

            var asyncObj = new AsyncObj();
            var context = new Dictionary<string, object>{
                { "x", asyncObj}
            };

            Utility.TestExpectedMarkupGenerated(input, expected, context);
            Assert.True(asyncObj.HasRun);
        }

        public class AsyncObj
        {
            public bool HasRun { get; set; }

            public async Task AsyncTask()
            {
                await Task.Yield();
                HasRun = true;
            }

            public async Task<int> AsyncTaskOfPrimative()
            {
                await Task.Yield();
                return 24;
            }

            public async Task<Guid> AsyncTaskOfValueType()
            {
                await Task.Yield();
                return new Guid("80658591-5dbe-438c-9e26-7233fd098749");
            }

            //Sometimes async methods might not actually execute any async code.
            public async Task TaskRanToCompletion()
            {
                return;
            }

            public Task<int> TaskOfPrimativeRanToCompletion()
            {
                return Task.FromResult(76);
            }

            public Task<Guid> TaskOfValueTypeRanToCompletion()
            {
                return Task.FromResult(new Guid("0b3b9ba1-2964-4de0-a3ff-626b19d91fe8"));
            }

        }


        internal class ManualAsyncExample
        {
            public static async Task<T> AwaitTask<T>(Task<T> task)
            {
                return await task;
            }


            //[AsyncStateMachine(typeof(AsyncTaskStateMachine<T>))]
            public static Task<T> ManualAwaitTask<T>(Task<T> task)
            {
                var stateMachine = new AsyncTaskStateMachine<T>();
                stateMachine.Builder = AsyncTaskMethodBuilder<T>.Create();
                stateMachine.Task = task;
                stateMachine.MoveNext();
                return stateMachine.Builder.Task;
            }


            private struct AsyncTaskStateMachine<T> : IAsyncStateMachine
            {
                public AsyncTaskMethodBuilder<T> Builder;
                public Task<T> Task;

                private TaskAwaiter<T> _awaiter;
                private State _state;

                public void MoveNext()
                {
                    T result;
                    try
                    {
                        switch (_state)
                        {
                            case State.Pending:
                                goto Label_GetResult;
                            case State.Complete:
                                return;
                            case State.Initial:
                                _awaiter = Task.GetAwaiter();
                                if (_awaiter.IsCompleted)
                                {
                                    goto Label_GetResult;
                                }
                                _state = State.Pending;
                                _awaiter.UnsafeOnCompleted(MoveNext);
                                Builder.AwaitUnsafeOnCompleted(ref _awaiter, ref this);
                                return;
                        }

                    Label_GetResult: 
                        result = _awaiter.GetResult();
                    }
                    catch (Exception e)
                    {
                        _state = State.Complete;
                        Builder.SetException(e);
                        return;
                    }
                    _state = State.Complete;
                    Builder.SetResult(result);
                }

                [DebuggerHidden]
                public void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    Builder.SetStateMachine(stateMachine);
                }
            }

            private enum State
            {
                Initial = 0,
                Complete = -1,
                Pending = 1
            }
        }

    }
}
