using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(GlobalMode.AsProvided)]
    [TestFixture(GlobalMode.Force)]
    public class SetTests : TemplateExeuctionBase
    {
        public SetTests(GlobalMode mode) : base(mode)
        {
        }

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

        [TestCase("#set($y=$undefined)")]
        [TestCase("#set($y=$null)")]
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

        [Test]
        public void ShouldSetFromVariable()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = 123
            };

            var input = "#set($output = $input)";
            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Contains.Item("output"));
            Assert.That(result.Context["output"], Is.EqualTo(123));
        }


        [Test]
        public void ShouldSetFromProperty()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = "Hello World"
            };
            var input = "#set($output = $input.Length)";
            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Contains.Item("output"));
            Assert.That(result.Context["output"], Is.EqualTo(11));
        }

        [Test]
        public void ShouldSetFromMethod()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = 7.52m
            };
            var input = "#set($output = $input.ToString())";
            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Contains.Item("output"));
            Assert.That(result.Context["output"], Is.EqualTo("7.52"));
        }

        [Test]
        public void ShouldSetFromMethodWithParameters()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = 7.578
            };
            var input = "#set($output = $input.ToString('F1'))";
            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Contains.Item("output"));
            Assert.That(result.Context["output"], Is.EqualTo("7.6"));
        }
        
        [Test]
        public void ShouldIgnoreSetToMethod()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = new PropertyTest()
            };
            var input = "#set($input.Method() = 123)";
            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Has.Count.EqualTo(1));
            Assert.That(result.Context.Keys, Contains.Item("input"));
        }

        //These tests deal with setting variables that have already been set.  The rules of global variables
        // do not allow these to be changed, so these tests can only be perfomed in GlobalMode.AsProvided
        public class SetNonGlobalTests : TemplateExeuctionBase
        {
            public SetNonGlobalTests() : base(GlobalMode.AsProvided)
            {
            }

            [TestCase("#set($x=123)")]
            public void When_SettingAVariableThatAlreadyExists_Should_OverwriteValueInContextAndRenderNothing(string input)
            {
                var context = new Dictionary<string, object>
                {
                    ["x"] = "hello world"
                };

                var result = ExecuteTemplate(input, context);

                Assert.That(result.Output, Is.Empty);
                Assert.That(result.Context.Keys, Has.Member("x"));
                Assert.That(result.Context["x"], Is.EqualTo(123));
            }

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

        }

        private class PropertyTest
        {
            public object Property { get; set; } = new object();
            public object Method() => null;
        }

    }
}
