using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Tests;

namespace IronVelocity.Tests.StaticTyping
{
    [TestFixture]
    public class ObjectArrayTests
    {
        [Test]
        public void EmptyArray()
        {
            var input = "[]";
            var context = new Dictionary<string, object>();

            var result = Test(input, context);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void ArrayWithSinglePrimitive()
        {
            var primitive = 623;
            var input = "[$primitive]";
            var context = new Dictionary<string, object>();
            context.Add("primitive", primitive);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            CollectionAssert.Contains(result, primitive);
        }

        [Test]
        public void ArrayWithSingleValueType()
        {
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");
            var input = "[$value]";

            var context = new Dictionary<string, object>();
            context.Add("value", value);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            CollectionAssert.Contains(result, value);
        }

        [Test]
        public void ArrayWithSingleReferenceType()
        {
            var reference = new StringBuilder();
            var input = "[$reference]";

            var context = new Dictionary<string, object>();
            context.Add("reference", reference);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            CollectionAssert.Contains(result, reference);
        }

        [Test]
        public void ArrayElementWithPropertyAccessor()
        {
            var value = "hello world";
            var input = "[$value.Length]";

            var context = new Dictionary<string, object>();
            context.Add("value", value);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            CollectionAssert.Contains(result, value.Length);
        }

        [Test]
        public void ArrayElementWithMethodCall()
        {
            var value = 45;
            var input = "[$value.ToString()]";

            var context = new Dictionary<string, object>();
            context.Add("value", value);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            CollectionAssert.Contains(result, value.ToString());
        }

        [Test]
        public void ArrayWithMixedItems()
        {
            var reference = new StringBuilder();
            var primitive = 623;
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");

            var input = "[$reference, $primitive, $value]";

            var context = new Dictionary<string, object>();
            context.Add("reference", reference);
            context.Add("primitive", primitive);
            context.Add("value", value);

            var result = Test(input, context);

            Assert.AreEqual(3, result.Count);
            CollectionAssert.Contains(result, reference);
            CollectionAssert.Contains(result, primitive);
            CollectionAssert.Contains(result, value);
        }



        private IList<object> Test(string input, IDictionary<string, object> context)
        {
            input = "#set($array = " + input + ")";

            var outputContext = Utility.Evaluate(input, context, globals: context);

            CollectionAssert.Contains(outputContext.Keys, "array");
            Assert.IsInstanceOf<IList<object>>(outputContext["array"]);

            return (IList<object>)outputContext["array"];

        }

    }
}
