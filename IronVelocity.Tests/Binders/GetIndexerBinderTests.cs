using System;
using System.Linq;
using IronVelocity.Binders;
using System.Dynamic;
using NUnit.Framework;
using IronVelocity.Reflection;
using System.Collections.Generic;

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

        [Test]
        public void ShouldGetIndexWithOneParameter()
        {
            var input = new IndexerTestHelper();
            var arg = new Guid("d182d744-e9f4-4811-9336-3d7d87b586a9");

            var result = GetIndexerTest(input, arg);

            Assert.That(result, Is.EqualTo(arg));
        }

        [Test]
        public void ShouldGetIndexWithTwoParameters()
        {
            var input = new IndexerTestHelper();

            var result = GetIndexerTest(input, 1.923, 4.73);

            Assert.That(result, Is.EqualTo("1.923 4.73"));
        }

        [Test]
        public void ShouldGetIndexWithParamsArgument()
        {
            var input = new IndexerTestHelper();

            var result = GetIndexerTest(input, 73, 917, 12, -19);

            Assert.That(result, Is.EqualTo("73, 917, 12, -19"));
        }

        [Test]
        public void ShouldGetIndexOnList()
        {
            var list = new List<string> { "foo", "bar" };

            var result = GetIndexerTest(list, 0);

            Assert.That(result, Is.EqualTo("foo"));
        }

        [Test]
        public void ShouldIgnoreSetterOnlyIndexer()
        {
            var input = new IndexerTestHelper();

            var result = GetIndexerTest(input, "Foo");

            Assert.That(result, Is.Null);
        }

        
        private object GetIndexerTest(object input, params object[] args)
        {
            var resolver = new IndexResolver(new OverloadResolver(new ArgumentConverter()));
            var binder = new VelocityGetIndexBinder(args.Length, resolver);
            args = new[] { input }.Concat(args).ToArray();

            return InvokeBinder(binder, args);
        }

        private class IndexerTestHelper
        {
            public Guid this[Guid  value] => value;
            public string this[double i1, double i2] => $"{i1} {i2}";
            public string this[params long[] values] => string.Join(", ", values);

            public object this[string str] { set { } }

        }
    }
}
