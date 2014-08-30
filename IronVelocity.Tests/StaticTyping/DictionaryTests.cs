using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Tests;

namespace IronVelocity.Tests.StaticTyping
{
    [TestFixture]
    public class DictionaryTests
    {
        [Test]
        public void EmptyDictionary()
        {
            var input = "%{}";
            var context = new Dictionary<string, object>();

            var result = Test(input, context);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void DictionaryWithSinglePrimitive()
        {
            var primitive = 623;
            var input = "%{primitive = $primitive}";
            var context = new Dictionary<string, object>();
            context.Add("primitive", primitive);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(result["primitive"], primitive);
        }

        [Test]
        public void DictionaryWithSingleValueType()
        {
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");
            var input = "%{value = $value}";

            var context = new Dictionary<string, object>();
            context.Add("value", value);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(result["value"], value);
        }

        [Test]
        public void DictionaryWithSingleReferenceType()
        {
            var reference = new StringBuilder();
            var input = "%{reference = $reference}";

            var context = new Dictionary<string, object>();
            context.Add("reference", reference);

            var result = Test(input, context);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(result["reference"], reference);
        }

        [Test]
        public void DictionaryWithMixedItems()
        {
            var reference = new StringBuilder();
            var primitive = 623;
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");

            var input = "%{reference = $reference, primitive = $primitive, value = $value}";

            var context = new Dictionary<string, object>();
            context.Add("reference", reference);
            context.Add("primitive", primitive);
            context.Add("value", value);

            var result = Test(input, context);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result["reference"], reference);
            Assert.AreEqual(result["primitive"], primitive);
            Assert.AreEqual(result["value"], value);
        }



        private IDictionary<object, object> Test(string input, IDictionary<string, object> context)
        {
            input = "#set($dictionary = \"" + input + "\")";

            var outputContext = Utility.Evaluate(input, context, globals:context);

            CollectionAssert.Contains(outputContext.Keys, "dictionary");
            Assert.IsInstanceOf<IDictionary<object, object>>(outputContext["dictionary"]);

            return (IDictionary<object, object>)outputContext["dictionary"];

        }

    }
}
