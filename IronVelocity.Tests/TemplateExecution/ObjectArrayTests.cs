using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture]
    public class ObjectArrayTests : TemplateExeuctionBase
    {
        [TestCase("[]")]
        [TestCase("[    ]")]
        public void EmptyArray(string input)
        {
            var result = EvaluateObjectArray(input);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ArrayWithSinglePrimitive()
        {
            var primitive = 623;
            var input = "[$primitive]";
            var context = new { primitive = primitive };

            var result = EvaluateObjectArray(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(primitive));
        }

        [Test]
        public void ArrayWithSingleValueType()
        {
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");

            var input = "[$value]";
            var context = new { value = value };

            var result = EvaluateObjectArray(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(value));
        }

        [Test]
        public void ArrayWithSingleReferenceType()
        {
            var reference = new object();

            var input = "[$reference]";
            var context = new { reference = reference };

            var result = EvaluateObjectArray(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(reference));
        }

        [Test]
        public void ArrayElementWithPropertyAccessor()
        {
            var value = "hello world";
            var input = "[$value.Length]";
            var context = new { value = value };

            var result = EvaluateObjectArray(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(value.Length));
        }

        [Test]
        public void ArrayElementWithMethodCall()
        {
            var value = 45;
            var input = "[$value.ToString()]";

            var context = new { value = value };

            var result = EvaluateObjectArray(input, context);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(value.ToString()));
        }

        [Test]
        public void ArrayWithMixedItems()
        {
            var reference = new StringBuilder();
            var primitive = 623;
            var value = new Guid("56357676-88b0-401f-b75b-5ab124268801");

            var input = "[$reference, $primitive, $value]";
            var context = new
            {
                value = value,
                reference = reference,
                primitive = primitive
            };

            var result = EvaluateObjectArray(input, context);

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(reference));
            Assert.That(result[1], Is.EqualTo(primitive));
            Assert.That(result[2], Is.EqualTo(value));
        }

        private IList<object> EvaluateObjectArray(string input, object context = null)
        {
            input = "#set($array = " + input + ")";

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Context.Keys, Contains.Item("array"));
            var result = execution.Context["array"];
            Assert.That(result, Is.InstanceOf<IList<object>>());

            return (IList<object>)result;

        }

    }
}
