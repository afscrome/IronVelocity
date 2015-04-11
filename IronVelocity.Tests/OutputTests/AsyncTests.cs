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
    [Explicit]
    public class GeneratedTypeTests
    {
        [Test]
        public async Task AsyncTask_Produces_No_Output()
        {
            var input = "$x.AsyncTask()";
            var expected = "";

            var asyncObj = new AsyncObj();
            var context = new Dictionary<string, object>{
                { "x", asyncObj}
            };

            await Utility.TestExpectedMarkupGeneratedAsync(input, expected, context);
            Assert.True(asyncObj.HasRun);
        }


        [Test]
        public async Task AsyncTask_IsContinued()
        {
            var input = "Before $x.AsyncTask() After";
            var expected = "Before  After";

            var asyncObj = new AsyncObj();
            var context = new Dictionary<string, object>{
                { "x", asyncObj}
            };

            await Utility.TestExpectedMarkupGeneratedAsync(input, expected, context);
            Assert.True(asyncObj.HasRun);
        }

        [Test]
        public async Task AsyncTask_IsCompleted()
        {
            var input = "Before $x.TaskRanToCompletion() After";
            var expected = "Before  After";

            var asyncObj = new AsyncObj();
            var context = new Dictionary<string, object>{
                { "x", asyncObj}
            };

            await Utility.TestExpectedMarkupGeneratedAsync(input, expected, context);
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
            public Task TaskRanToCompletion()
            {
                HasRun = true;
                return Task.FromResult(true);
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



    }
}
