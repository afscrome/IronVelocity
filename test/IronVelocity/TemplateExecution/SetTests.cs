using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class SetTests : TemplateExeuctionBase
    {
        public SetTests(StaticTypingMode mode) : base(mode)
        {
        }

        [Test]
        public void ShouldSetVariable()
        {
            var input = "#set($foo = 'bar')";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Has.Member("foo"));
            Assert.That(result.Context["foo"], Is.EqualTo("bar"));
        }

        [Test]
        public void ShouldSetProperty()
        {
            var item = new SetTestHelper();
            var locals = new
            {
                item = item
            };

            var input = "#set($item.Property = 'Foo')";

            var result = ExecuteTemplate(input, locals);
            Assert.That(result.Output, Is.Empty);
            Assert.That(item.Property, Is.EqualTo("Foo"));
        }

        [Test]
        public void ShouldSetIndex()
        {
            var item = new SetTestHelper();
            var locals = new
            {
                item = item
            };

            var input = "#set($item[8323] = 'FizzBuzz')";
            var result = ExecuteTemplate(input, locals);
            Assert.That(result.Output, Is.Empty);
            Assert.That(item[8323], Is.EqualTo("FizzBuzz"));
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
        public void ShouldSetFromIndex()
        {
            var context = new
            {
                array = new[] { "Hello", "World" }
            };

            var input = "#set($output = $array[1])";
            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Contains.Item("output"));
            Assert.That(result.Context["output"], Is.EqualTo("World"));
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

        [TestCase("#set($undefined=$undefined)")]
        [TestCase("#set($undefined=$null)")]
        public void ShouldIgnoreNullAssignmentToNotPresentVariable(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["null"] = null
            };

            var result = ExecuteTemplate(input, context);
            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context, Has.Count.EqualTo(1));
        }

        [TestCase("#set($alreadySet=$undefined)")]
        [TestCase("#set($alreadySet=$null)")]
        public void ShouldIgnoreNullAssignmentToExistingVariable(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["alreadySet"] = 937,
                ["null"] = null
            };

            //Explicitly provide no globals so alreadySet is treated as a local, not a global variable
            var result = ExecuteTemplate(input, locals: context, globals: new { });
            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context, Has.Count.EqualTo(2));
            Assert.That(result.Context["alreadySet"], Is.EqualTo(937));
        }


        [TestCase("#set($item.Property=$undefined)")]
        [TestCase("#set($item.Property=$null)")]
        public void ShouldIgnoreNullAssignmentToProperty(string input)
        {
            var item = new { Property = new object() };
            var originalPropertyValue = item.Property;

            var context = new Dictionary<string, object>
            {
                ["null"] = null
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(item.Property, Is.EqualTo(originalPropertyValue));
        }

        [TestCase("#set($item['fake'] =$undefined)")]
        [TestCase("#set($item['fake'] = $null)")]
        public void ShouldIgnoreNullAssignmentToIndex(string input)
        {
            var item = new SetTestHelper();
            item[27] = "Fake";

            var context = new Dictionary<string, object>
            {
                ["item"] = item,
                ["null"] = null
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(item[27], Is.EqualTo("Fake"));
        }

        [TestCase("#set($fake.NonExistantProperty=123)")]
        [TestCase("#set($undefined.Property=123)")]
        public void ShouldIgnoreAssignmentToInvalidProperty(string input)
        {
            var context = new
            {
                fake = new object()
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context, Has.Count.EqualTo(1));
        }

        [TestCase("#set($array['foo']=928)")]
        [TestCase("#set($undefined['foo']=328)")]
        public void ShouldIgnoreAssignmentToInvalidIndex(string input)
        {
            var context = new
            {
                array = new int[0]
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context, Has.Count.EqualTo(1));
        }

        [Test]
        public void ShouldIgnorePropertyAssignmentOfIncompatibleType()
        {
            var item = new SetTestHelper();
            var locals = new
            {
                item = item
            };

            var input = "#set($item.Property = 123)";

            var result = ExecuteTemplate(input, locals);

            Assert.That(result.Output, Is.Empty);
            Assert.That(item.Property, Is.Null);
        }

        [Test]
        public void ShouldIgnoreIndexAssignmentOfIncompatibleType()
        {
            var item = new SetTestHelper();
            var locals = new
            {
                item = item
            };

            var input = "#set($item[123] = true)";

            var result = ExecuteTemplate(input, locals);
            Assert.That(result.Output, Is.Empty);
            Assert.That(item._indexValues, Is.Empty);
        }

        [Test]
        public void ShouldIgnoreSetToMethod()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = new SetTestHelper()
            };
            var input = "#set($input.Method() = 123)";
            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Context.Keys, Has.Count.EqualTo(1));
            Assert.That(result.Context.Keys, Contains.Item("input"));
        }

        //These tests deal with setting variables that have already been set.  The rules of global variables
        // do not allow these to be changed, so these tests can only be performed in GlobalMode.AsProvided
        public class SetNonGlobalTests : TemplateExeuctionBase
        {
            public SetNonGlobalTests() : base(StaticTypingMode.AsProvided)
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

        public class SetTestHelper
        {
            public IDictionary<object, string> _indexValues = new Dictionary<object, string>();

            public string this[object key]
            {
                get { return _indexValues[key]; }
                set { _indexValues[key] = value; }
            }

            public string Property { get; set; }
            public object Method() => null;
        }

    }
}
