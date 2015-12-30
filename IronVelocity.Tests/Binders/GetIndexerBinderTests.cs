using System;
using System.Linq;
using IronVelocity.Binders;
using System.Dynamic;
using NUnit.Framework;
using IronVelocity.Reflection;

namespace IronVelocity.Tests.Binders
{
    public class GetIndexerBinderTests : BinderTestBase
    {
        [Test]
        public void ShouldIndexIntoSingleDimensionalPrimitiveArray()
        {
            var array = new int[] { 1, 2, 3 };

            var result = GetIndexerTest(array, 1);

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void ShouldIndexIntoSingleDimensionalStructArray()
        {
            var array = new Guid[] {
                new Guid("70ae656b-4826-47fb-9f23-ce3f8b85dfb0"),
                new Guid("5b001171-8db6-47db-9830-0bab734c7d19"),
                new Guid("af90933f-6e89-4c50-9136-3bd422cd7daa"),
                new Guid("0eb4bc26-e77b-444e-84db-2239a936a771")
            };

            var result = GetIndexerTest(array, 2);

            Assert.That(result, Is.EqualTo(new Guid("af90933f-6e89-4c50-9136-3bd422cd7daa")));
        }

        [Test]
        public void ShouldIndexIntoSingleDimensionalReferenceTypeArray()
        {
            var array = new object[] {
                "foo",
                123
            };

            var result = GetIndexerTest(array, 0);

            Assert.That(result, Is.EqualTo("foo"));
        }

        [Test]
        public void ShouldIndexIntoTwoDimensionalArray()
        {
            var array = new int[,] {
                {1,2,3,4},
                {5,6,7, 8},
                {9,10,11,12}
            };

            var result = GetIndexerTest(array, 1, 3);

            Assert.That(result, Is.EqualTo(8));
        }
        [Test]
        public void ShouldFailToIndexArrayWithNonIntegerArguments()
        {
            var array = new double[] { 3.14, 8.92 };

            var result = GetIndexerTest(array, "fizz");

            Assert.That(result, Is.Null);
        }


        private object GetIndexerTest(object input, params object[] args)
        {
            var resolver = new IndexResolver();
            var binder = new VelocityGetIndexBinder(resolver, new CallInfo(args.Length));
            args = new[] { input }.Concat(args).ToArray();

            return InvokeBinder(binder, args);
        }
    }
}
