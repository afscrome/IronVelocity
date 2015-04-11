using IronVelocity.Binders;
using NUnit.Framework;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.Binders
{

    /*
        Task<T> test()
        * returns T if completed
        * returns Task<T> if not complete

        Task test()
        * returns void if completed
        * returns Task if not completed


        string test(string s)
        * returns string if s is string
        * returns string if s is completed Task<string>
        * returns Task<string> if s is uncompleted Task<string>


        void test(string s)
        * returns void if s is string
        * returns void if s is completed Task<string>
        * returns Task<string> if s is uncompleted Task<string>

     */

    [Explicit("Async work in progress")]
    public class InvokeMemberBinderAsyncTests
    {

        [Test]
        public void TaskOfTResultIsReducedtoTIffCompleted()
        {
            var target = new AsyncTestHelper();
            var result = test(target, "SimpleTaskOfT", true);

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual("Hello World", result);
        }

        [Test]
        public async Task TaskOfTResultIsTaskOfTWhenYielded()
        {
            var target = new AsyncTestHelper();
            var result = test(target, "SimpleTaskOfT", true);

            Assert.IsInstanceOf<Task<string>>(result);
            var awaitedResult = await (Task<string>)result;
            Assert.AreEqual("Hello World", awaitedResult);
        }

        [Test]
        public void TaskIsReducedToVoidIfCompleted()
        {
            var target = new AsyncTestHelper();
            var result = test(target, "SimpleTask", false);

            Assert.Inconclusive();
            //Assert.IsInstanceOf<void>(result);
        }

        [Test]
        public void TaskResultIsTaskWhenYielded()
        {
            var target = new AsyncTestHelper();
            var result = test(target, "SimpleTask", true);

            Assert.IsInstanceOf<Task>(result);
        }


        [Test]
        public async Task BinderCacheNotPollutedForTaskOfT()
        {
            TaskOfTResultIsReducedtoTIffCompleted();
            await TaskOfTResultIsTaskOfTWhenYielded();
            TaskOfTResultIsReducedtoTIffCompleted();
            await TaskOfTResultIsTaskOfTWhenYielded();
        }

        [Test]
        public void BinderCacheNotPollutedForTask()
        {
            TaskIsReducedToVoidIfCompleted();
            TaskResultIsTaskWhenYielded();
            TaskIsReducedToVoidIfCompleted();
            TaskResultIsTaskWhenYielded();
        }


        private object test(object input, string methodName, params object[] paramaters)
        {
            var binder = new VelocityInvokeMemberBinder(methodName, new CallInfo(paramaters.Length));

            var args = new[] { input }.Concat(paramaters).ToArray();

            return Utility.BinderTests(binder, args);
        }

        public class AsyncTestHelper
        {
            public async Task<string> SimpleTaskOfT(bool yield)
            {
                if (yield)
                {
                    await Task.Yield();
                }
                return "Hello World";
            }

            public async Task SimpleTask(bool isCompleted)
            {
                if (!isCompleted)
                {
                    await Task.Yield();
                }
            }

            public string DoubleString(string input)
            {
                return input + input;
            }

            public void DoStuff(string input)
            {
            }

        }
    }
}
