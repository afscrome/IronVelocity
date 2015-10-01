using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace IronVelocity.Tests.TemplateExecution
{
    [Explicit]
    public class RegressionTests : TemplateExeuctionBase
    {
        private static readonly string _base = "..\\..\\Regression\\templates\\";
        private static readonly string _failureResultsDir = "Failures";

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            if (!Directory.Exists(_failureResultsDir))
                Directory.CreateDirectory(_failureResultsDir);
            else
            {
                foreach (var file in Directory.GetFiles(_failureResultsDir, "*"))
                    File.Delete(file);
            }
        }

        [TestCaseSource(nameof(TestsCases))]
        public void RegressionTest(string name)
        {
            var inputPath = Path.Combine(_base, name + ".vm");
            var expectedPath = Path.Combine(_base, "Expected", name + ".cmp");

            if (!File.Exists(inputPath))
                Assert.Inconclusive($"Input file does not exist at {inputPath}");

            if (!File.Exists(expectedPath))
                Assert.Inconclusive($"Expected result does not exist at {expectedPath}");

            var input = File.ReadAllText(inputPath);
            var expected = Utility.NormaliseLineEndings(File.ReadAllText(expectedPath));
            var context = CreateContext(name);

            var result = ExecuteTemplate(input, context);

            try
            {
                Assert.That(result.OutputWithNormalisedLineEndings, Is.EqualTo(expected));
            }
            catch (AssertionException)
            {
                if (result.OutputWithNormalisedLineEndings.Replace(" ", "").Replace("\t", "") == expected.Replace(" ", "").Replace("\t", ""))
                {
                    Assert.Fail("Differ only by whitespace");
                }
                var failurePath = Path.Combine(_failureResultsDir, name);
                File.WriteAllText(failurePath + ".expected", expected);
                File.WriteAllText(failurePath + ".actual", result.OutputWithNormalisedLineEndings);
                File.WriteAllText(failurePath + ".input", input);

                throw;
            }
        }

        private IDictionary<string, object> CreateContext(string name)
        {
            var provider = new Provider();



            var context = new Dictionary<string, object>
            {
                ["provider"] = provider,
                ["stringarray"] = new[] { "first element", "second element" },
                ["list"] = provider.Customers,
                ["hashtable"] = new Dictionary<string, string>
                {
                    ["Bar"] = "this is from a hashtable!",
                    ["Foo"] = "this is from a hashtable too!"
                },
                ["hashmap"] = new Dictionary<string,object>(),
                ["name"] = "jason",
            };

            return context;
        }


        public static IEnumerable<TestCaseData> TestsCases
        {
            get
            {
                foreach (var file in Directory.GetFiles(_base, "*.vm"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var testCase = new TestCaseData(name)
                        .SetName("Regression Test ANTLR: " + name + ".vm");

                    if (name.StartsWith("velocimacro") || name.StartsWith("vm_test"))
                    {
                        testCase.Ignore("Global Velocimacros not supported");
                    }
                    else if (name == "escape2" || name == "include")
                    {
                        testCase.Ignore("Include not supported");
                    }
                    else if (name == "parse")
                    {
                        testCase.Ignore("Parse not supported");
                    }

                    yield return testCase;
                }
            }
        }


        private class Provider
        {
            public string this[string key] => key;

            public string Chop(string input, int count) => input.Substring(0, input.Length - count);
            public ArrayList Customers => new ArrayList { "ArrayList element 1", "ArrayList element 2", "ArrayList element 3", "ArrayList element 4" };
            public string Title => "lunatic";

            //Yes, I have no idea why some concat operations use space seperators, but others don't
            public string concat(IEnumerable<object> args) => string.Join(" ", args);
            public string concat(string first, string second) => first + second;

            public Hashtable Hashtable => new Hashtable
            {
                ["key0"] = "value0",
                ["key1"] = "value1",
                ["key2"] = "value2",
            };


            public override string ToString() => "test provider";

            public Person Person => new Person();
            public Child Child => new Child();

            public string ShowPerson(Person person) => person.Name;

        }
        private class Person {
            public virtual string Name => "Person";
        }
        private class Child : Person {
            public override string Name => "Child";
        }

    }
}
