using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    public class SetTests : TemplateExeuctionBase
    {
        [TestCase("#set($x=123)")]
        /*
        [TestCase("#set($x = 123)\r\n", IgnoreReason = "TODO: Implement correct Whitespace Eating")]
        [TestCase("#set($x = 123)   \r\n", IgnoreReason = "TODO: Implement correct Whitespace Eating")]
        [TestCase(" #set($x = 123)", IgnoreReason = "TODO: Implement correct Whitespace Eating")]
        [TestCase("\t \t\t  #set($x = 123)", IgnoreReason = "TODO: Implement correct Whitespace Eating")]
        */
        public void When_SettingAVariable_Should_StoreInContextAndRenderNothing(string input)
        {
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Has.Member("x"));
            Assert.That(result.Context["x"], Is.EqualTo(123));
        }

        [TestCase("#set($x=123)")]
        public void When_SettingAVariableThatAlreadyExists_Should_OverwriteValueInContextAndRenderNothing(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["x"] = "hello world"
            };

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Has.Member("x"));
            Assert.That(result.Context["x"], Is.EqualTo(123));
        }

        [TestCase("#set($y=$undefined)")]
        [TestCase("#set($y=$null)")]
        [TestCase("#set($alreadySet=$undefined)")]
        [TestCase("#set($alreadySet=$null)")]
        public void When_SettingAVariableWithNullOrUndefined_Should_NeitherChangeContextNorRenderAnything(string input)
        {
            var originalAlreadySetValue = new object();
            var context = new Dictionary<string, object>
            {
                ["alreadySet"] = originalAlreadySetValue,
                ["null"] = null
            };

            var result = ExecuteTemplate(input, context);
            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Has.No.Member("x"));
            Assert.That(result.Context["alreadySet"], Is.EqualTo(originalAlreadySetValue));
        }

        [TestCase("#set($fake.Property=123)")]
        [TestCase("#set($y.Property=123)")]
        public void When_SettingAPropertyThatDoesNotExist_Should_NeitherChangeContextNorRenderAnything(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["item"] = 123,
                ["null"] = null
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context, Has.Count.EqualTo(2));
        }

        [TestCase("#set($item.Property=$undefined)")]
        [TestCase("#set($item.Property=$null)")]
        public void When_SettingAPropertyWithNullOrUndefined_Should_NeitherChangePropertyValueOrRenderAnything(string input)
        {
            var item = new { Property = new object() };
            var originalPropertyValue = item.Property;

            var context = new Dictionary<string, object>
            {
                ["y"] = item,
                ["null"] = null
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(item.Property, Is.EqualTo(originalPropertyValue));
        }


        private class PropertyTest
        {
            public object Property { get; set; } = new object();
        }

    }
}
