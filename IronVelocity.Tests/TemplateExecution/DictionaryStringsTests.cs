using NUnit.Framework;
using System;
using System.Collections;
using System.Text;

namespace IronVelocity.Tests.TemplateExecution
{
    public class DictionaryStringsTests : TemplateExeuctionBase
    {
        [TestCase("%{}")]
        [TestCase("%{    }")]
        public void ShouldProcessEmptyDictionary(string input)
        {
            var result = EvaluateDictionary(input);

            Assert.That(result, Is.Empty);
        }

        [TestCase("%{key='value'}")]
        [TestCase("%{  key='value'}")]
        [TestCase("%{key   ='value'}")]
        [TestCase("%{  key ='value'}")]
        [TestCase("%{key='value'}")]
        [TestCase("%{key=  'value'}")]
        [TestCase("%{key='value'  }")]
        [TestCase("%{key=  'value'  }")]
        [TestCase("%{  key  =  'value'  }")]
        public void ShouldProcessDictionaryWithWhitespaceAroundKeysOrValuesWithConstantValues(string input)
        {
            var result = EvaluateDictionary(input);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("key"));
            Assert.That(result["key"], Is.EqualTo("value"));
        }

        [TestCase("'hello world'", "hello world")]
        [TestCase("'hello $x world'", "hello beautiful world")]
        [TestCase("'$i'", (int)72)] //This doesn't make sense to me, but is what NVelocity does, so maintain for backwards compatability.
        [TestCase("'$i$i'", "7272")]
        [TestCase("97", (int)97)]
        [TestCase("44.67", (float)44.67)]
        [TestCase("$x", "beautiful")]
        public void ShouldProcessDictionaryWithSIngleValueTypeValue(string input, object expected)
        {
            var dictionary = "%{ key = " + input + "}";
            var env = new
            {
                i = 72,
                x = "beautiful",
            };

            var result = EvaluateDictionary(dictionary, env);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("key"));
            Assert.That(result["key"], Is.EqualTo(expected));
        }

        [Test]
        public void ShouldProcessDictionaryWithSinglePrimitiveValue()
        {
            var primitive = 623;
            var input = "%{primitive = $primitive}";
            var context = new { primitive = primitive };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["primitive"], Is.EqualTo(primitive));
        }

        [Test]
        public void ShouldProcessDictionaryWithSingleValueType()
        {
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");
            var input = "%{value = $value}";
            var context = new { value = value };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["value"], Is.EqualTo(value));
        }

        [Test]
        public void ShouldProcessDictionaryWithSingleReferenceTypeValue()
        {
            var reference = new StringBuilder();
            var input = "%{reference = $reference}";
            var context = new { reference = reference };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["reference"], Is.EqualTo(reference));
        }

        [Test]
        public void ShouldProcessDictionaryWithSingleInterpolatedValue()
        {
            var input = "%{interpolated = 32$value}";
            var context = new { value = "fizzbuzz"};

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["interpolated"], Is.EqualTo("32fizzbuzz"));
        }

        [Test]
        public void ShouldProcessDictionaryStringWithNestedDictionary()
        {
            var input = "%{ dict = { key = 123} }";

            var result = EvaluateDictionary(input);

            Assert.That(result["dict"], Is.InstanceOf<IDictionary>());

            var nested = (IDictionary)result["dict"];
            Assert.That(nested, Has.Count.EqualTo(1));
            Assert.That(nested["key"], Is.EqualTo(123));
        }

        public void ShouldProcessDictionaryStringWithEscapeChars()
        {
            var input = "%{ quoted = '\\'Hello\\' he said'}";
            var result = EvaluateDictionary(input);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["quoted"], Is.EqualTo("'Hello' he said"));
        }


        [Test]
        public void ShouldProcessDictionaryStringValueWithDictionaryLikeTextAsString()
        {
            var input = "%{ value = '{ a: b}'}";
            var result = EvaluateDictionary(input);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["value"], Is.EqualTo("{ a: b}"));
        }

        [Test]
        public void ShouldProcessDictionaryWithMixedItems()
        {
            var reference = new StringBuilder();
            var primitive = 623;
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");

            var input = "%{reference = $reference, primitive = $primitive, value = $value, interpolated = 3$primitive}";

            var context = new
            {
                reference = reference,
                primitive = primitive,
                value = value
            };



            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result["reference"], Is.EqualTo(reference));
            Assert.That(result["primitive"], Is.EqualTo(primitive));
            Assert.That(result["value"], Is.EqualTo(value));
            Assert.That(result["interpolated"], Is.EqualTo("3623"));
        }

        private IDictionary EvaluateDictionary(string input, object context = null)
        {
            input = $"#set($dict = \"{input}\")";

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Context.Keys, Contains.Item("dict"));
            var result = execution.Context["dict"];
            Assert.That(result, Is.InstanceOf<IDictionary>());

            return (IDictionary)result;
        }

    }
}
