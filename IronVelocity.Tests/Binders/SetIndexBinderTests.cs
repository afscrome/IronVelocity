using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System;
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
            var array = new float[4,10];

            SetIndexTest(array, 3.142f, 3, 9);

            Assert.That(array[3, 9], Is.EqualTo(3.142f));
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


        private void SetIndexTest(object target, object value, params object[] args)
        {
            var resolver = new IndexResolver(new OverloadResolver(new ArgumentConverter()));
            var binder = new VelocitySetIndexBinder(resolver, args.Length);
            args = new[] { target }.Concat(args).Concat(new[] { value }).ToArray();

            InvokeBinder(binder, args);
        }

    }
}
