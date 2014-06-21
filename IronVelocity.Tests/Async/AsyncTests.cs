using IronVelocity.Compilation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.Async
{
    [TestFixture]
    public class AsyncTests
    {
        [Test]
        public void SetStateMachineTest()
        {
            var typeBuilder = NewTypeBuilder();

            var asyncTaskMethodBuilder = typeBuilder.DefineField("AsyncTaskMethodBuilder", typeof(AsyncTaskMethodBuilder), FieldAttributes.Public);
            VelocityAsyncCompiler.BuildSetStateMachineMethod(typeBuilder, asyncTaskMethodBuilder);

            var type = typeBuilder.CreateType();

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("SetStateMachine");

            method.Invoke(instance, new object[] { new FakeStateMachine() });
        }

        [Test]
        public void MoveNextTest()
        {
            var typeBuilder = NewTypeBuilder();

            VelocityAsyncCompiler.BuildMoveNextMethod(typeBuilder);

            var type = typeBuilder.CreateType();

            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod("MoveNext");

            method.Invoke(instance, new object[0]);
        }


        [Test]
        public void ImplmentsIAsyncStateMachine()
        {
            var typeBuilder = NewTypeBuilder();

            VelocityAsyncCompiler.BuildStateMachine(typeBuilder);

            var type = typeBuilder.CreateType();

            Assert.True(typeof(IAsyncStateMachine).IsAssignableFrom(type));
        }

        private TypeBuilder NewTypeBuilder()
        {
            var name = new AssemblyName("TestAssembly");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("TestModule", true);
            return moduleBuilder.DefineType("TestType", TypeAttributes.Public | TypeAttributes.SequentialLayout);
        }



        private struct FakeStateMachine : IAsyncStateMachine
        {
            public void MoveNext()
            {
                throw new NotImplementedException();
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                throw new NotImplementedException();
            }
        }

    }
}
