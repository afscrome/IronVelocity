using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.StaticTyping
{
    [TestFixture]
    public class SetTests
    {

        [Test]
        public void SetWithSinglePrimitive()
        {
            var primitive = 623;
            var input = "$primitive";
            var context = new Dictionary<string, object>();
            context.Add("primitive", primitive);

            var result = Test<int>(input, context);

            Assert.AreEqual(primitive, result);
        }

        [Test]
        public void SetWithSingleValueType()
        {
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");
            var input = "$value";

            var context = new Dictionary<string, object>();
            context.Add("value", value);

            var result = Test<Guid>(input, context);

            Assert.AreEqual(value, result);
        }

        [Test]
        public void SetWithSingleReferenceType()
        {
            var reference = new StringBuilder();
            var input = "$reference";

            var context = new Dictionary<string, object>();
            context.Add("reference", reference);

            var result = Test<StringBuilder>(input, context);

            Assert.AreEqual(reference, result);
        }


        [Test]
        public void SetWithMethodCall()
        {
            var value = 456;
            var input = "$value.ToString()";

            var context = new Dictionary<string, object>();
            context.Add("value", value);

            var result = Test<string>(input, context);

            Assert.AreEqual("456", result);
        }

        [Test]
        public void SetWithPropertyAccess()
        {
            var value = "boo";
            var input = "$value.Length";

            var context = new Dictionary<string, object>();
            context.Add("value", value);

            var result = Test<int>(input, context);
            Assert.AreEqual(3, result);
        }

        private T Test<T>(string input, IDictionary<string, object> context)
        {
            input = "#set($result = "+ input + ")";

            var outputContext = Utility.Evaluate(input, context, globals: context);

            CollectionAssert.Contains(outputContext.Keys, "result");
            Assert.IsInstanceOf<T>(outputContext["result"]);

            return (T)outputContext["result"];

        }

    }
}
