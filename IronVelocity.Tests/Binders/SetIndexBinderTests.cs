using IronVelocity.Binders;
using IronVelocity.Reflection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.Tests.Binders
{
    public class SetIndexBinderTests : BinderTestBase
    {
        [Test]
        public void ShouldSetIndexOnSingleDimensionPrimitiveArray()
        {
            var array = new int[5];

            SetIndexTest(array, 67, 4);

            Assert.That(array[4], Is.EqualTo(67));
        }

        [Test]
        public void ShouldSetIndexOnSingleDimensionalStructArray()
        {
            var array = new Guid[9];
            var element = new Guid("69b31162-fdea-4b27-bcf4-279678d9592b");

            SetIndexTest(array, element, 7);

            Assert.That(array[7], Is.EqualTo(element));
        }

        [Test]
        public void ShouldSetIndexOnSingleDimensionalReferenceTypeArray()
        {
            var array = new Exception[4];
            var element = new Exception("This is a test");

            SetIndexTest(array, element, 0);

            Assert.That(array[0], Is.EqualTo(element));
        }

        [Test]
        public void ShouldSetIndexOnTwoDimensionalArray()
        {
            var array = new float[4, 10];

            SetIndexTest(array, 3.142f, 3, 9);

            Assert.That(array[3, 9], Is.EqualTo(3.142f));
        }

        [Test]
        public void ShouldNotSetIndexWithNullValueOnOnArray()
        {
            var array = new string[] { "foo" };

            SetIndexTest(array, null, 0);

            Assert.That(array[0], Is.EqualTo("foo"));
        }

        [Test]
        public void ShouldIgnoreNullTarget()
        {
            Assert.DoesNotThrow(() =>
            {
                SetIndexTest(null, "bar", 1);
            });
        }


        [Test]
        public void ShouldNotSetNullValue()
        {
            var array = new object[10];
            array[5] = "foo";

            SetIndexTest(array, null, 5);

            Assert.That(array[5], Is.EqualTo("foo"));
        }

        [Test]
        public void ShouldSetIndexWithOneParameter()
        {
            var mock = new Mock<ISetIndexTestHelper>();

            SetIndexTest(mock.Object, "value", "key");

            mock.VerifySet(x => x["key"] = "value", Times.Once);
        }

        [Test]
        public void ShouldSetIndexWithTwoParameters()
        {
            var mock = new Mock<ISetIndexTestHelper>();

            SetIndexTest(mock.Object, 10.543M, 123, 4.5f);

            mock.VerifySet(x => x[123, 4.5f] = 10.543M, Times.Once);
        }

        [Test]
        public void ShouldSetIndexWithParamsArgument()
        {
            //Can't use mock here due to https://github.com/Moq/moq4/issues/235
            var input = new SetIndexParamArrayHelper();

            SetIndexTest(input, 4.827, 7, 8, 9, 2);

            Assert.That(input.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void ShouldSetIndexOnList()
        {
            var list = new List<string> { "fizz", "buzz" };

            SetIndexTest(list, "updated", 1);

            Assert.That(list[1], Is.EqualTo("updated"));
        }

        [Test]
        public void ShouldNotSetNullValueOnCustomIndexerArray()
        {
            var mock = new Mock<ISetIndexTestHelper>();

            SetIndexTest(mock.Object, null, "key");

            mock.VerifySet(x => x[It.IsAny<string>()] = It.IsAny<string>(), Times.Never);
        }

        private void SetIndexTest(object target, object value, params object[] args)
        {
            var resolver = new IndexResolver(new OverloadResolver(new ArgumentConverter()));
            var binder = new VelocitySetIndexBinder(args.Length, resolver);
            args = new[] { target }.Concat(args).Concat(new[] { value }).ToArray();

            InvokeBinder(binder, args);
        }

        public interface ISetIndexTestHelper
        {
            string this[string s] { set; }
            decimal this[long l, float f] { set; }
            double this[params int[] values] { set; }
        }

        public class SetIndexParamArrayHelper
        {
            public int CallCount = 0;
            public double this[params int[] args]
            {
                set
                {
                    if (value == 4.827 && args[0] == 7 && args[1] == 8 && args[2] == 9 && args[3] == 2)
                        CallCount++;
                    else
                        throw new ArgumentOutOfRangeException();
                }

            }

        }

    }
}
