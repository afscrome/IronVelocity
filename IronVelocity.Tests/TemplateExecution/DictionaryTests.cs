using NUnit.Framework;
using System;
using System.Collections;
using System.Text;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class DictionaryTests : TemplateExeuctionBase
    {
        public DictionaryTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase("{}")]
        [TestCase("{    }")]
        public void ShouldProcessEmptyDictionary(string input)
        {
            var result = EvaluateDictionary(input);

            Assert.That(result, Is.Empty);
        }


        [Test]
        public void ShouldProcessDictionaryWithSingleConstantValue()
        {
            var input = "{constant : 'FooBar'}";

            var result = EvaluateDictionary(input);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("constant"));

            Assert.That(result["constant"], Is.EqualTo("FooBar"));
        }

        [Test]
        public void ShouldProcessDictionaryWithSinglePrimitiveValue()
        {
            var input = "{primitive : $Primitive}";
            var context = new { Primitive = 'f' };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("primitive"));

            Assert.That(result["primitive"], Is.EqualTo(context.Primitive));
        }

        [Test]
        public void ShouldProcessDictionaryWithSingleValueTypeValue()
        {
            var input = "{value : $ValueType}";
            var context = new { ValueType = Guid.NewGuid() };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("value"));

            Assert.That(result["value"], Is.EqualTo(context.ValueType));
        }

        [Test]
        public void ShouldProcessDictionaryWithSingleReferenceTypeValue()
        {
            var input = "{ref : $reference}";
            var context = new { Reference = new object() };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("ref"));

            Assert.That(result["ref"], Is.EqualTo(context.Reference));
        }

        [Test]
        public void ShouldProcessDictionaryWithPropertyAccessorValue()
        {
            var input = "{property : $string.Length}";
            var context = new { String = "Hello World" };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("property"));

            Assert.That(result["property"], Is.EqualTo(11));
        }

        [Test]
        public void ShouldProcessDictionaryWithMixedItems()
        {
            var input = "{value: $value, primitive : $primitive, ref: $reference}";
            var context = new
            {
                Value = Guid.NewGuid(),
                Primitive = 54.32,
                Reference = new object()
            };

            var result = EvaluateDictionary(input, context);

            Assert.That(result.Keys, Is.EquivalentTo(new[] { "primitive", "value", "ref" }));
            Assert.That(result["value"], Is.EqualTo(context.Value));
            Assert.That(result["primitive"], Is.EqualTo(context.Primitive));
            Assert.That(result["ref"], Is.EqualTo(context.Reference));
        }

        [Test]
        public void ShouldProcessDictionaryWithConstantKey()
        {
            var input = "{'Key' : true}";

            var result = EvaluateDictionary(input);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("Key"));

            Assert.That(result["Key"], Is.EqualTo(true));
        }

        [Test]
        public void ShouldProcessDictionaryWithInterpolatedStringKey()
        {
            var input = "{\"Foo${suffix}\" : true}";
            var context = new { Suffix = "Bar" };

            var result = EvaluateDictionary(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("FooBar"));

            Assert.That(result["FooBar"], Is.EqualTo(true));
        }

        [Test]
        public void ShouldProcessDictionaryWithIdentifierKey()
        {
            var input = "{ identifier : true}";
            var context = new { Suffix = "Bar" };

            var result = EvaluateDictionary(input);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Keys, Contains.Item("identifier"));

            Assert.That(result["identifier"], Is.EqualTo(true));
        }

        private IDictionary EvaluateDictionary(string input, object context = null)
        {
            input = $"#set($result = {input})";

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.InstanceOf<IDictionary>());

            return (IDictionary)result;
        }

    }
}
