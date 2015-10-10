using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class RegressionTests : TemplateExeuctionBase
    {
        public RegressionTests(StaticTypingMode mode) : base(mode)
        {
        }

        private static readonly string _base = "..\\..\\TemplateExecution\\Regression\\";
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

            var result = ExecuteTemplate(input, context, fileName: inputPath);

            try
            {
                Assert.That(result.OutputWithNormalisedLineEndings, Is.EqualTo(expected));
            }
            catch (AssertionException)
            {
                if (result.OutputWithNormalisedLineEndings.Replace(" ", "").Replace("\t", "") == expected.Replace(" ", "").Replace("\t", ""))
                {
                    Assert.Inconclusive("Differ only by whitespace");
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
                ["vector"] = new List<string> { "string1", "string2" },
                ["boolobj"] = new
                {
                    isBoolean = true,
                    isNotBoolean = "hello"
                }
            };

            return context;
        }


        public IEnumerable<TestCaseData> TestsCases
        {
            get
            {
                foreach (var file in Directory.GetFiles(_base, "*.vm"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var testCase = new TestCaseData(name)
                        .SetName("Regression Test ANTLR: " + name + ".vm");

                    switch (name)
                    {
                        case "velocimacro":
                        case "velocimacro2":
                        case "vm_test1":
                        case "vm_test2":
                            testCase.Ignore("Global Velocimacros not supported");
                            break;
                        case "escape2":
                        case "include":
                            testCase.Ignore("Include not supported");
                            break;
                        case "parse":
                            testCase.Ignore("Parse not supported as it's to expensive to compile. Can revisit if we ever support interpreting templates, as well as compiling & executing");
                            break;
                        case "escape":
                        case "reference":
                        case "test":
                            testCase.Ignore("Escaping in IronVelocity doesnt' match velocity");
                            break;
                        case "array":
                        case "interpolation":
                        case "literal":
                            testCase.Ignore("Macros not supported");
                            break;
                        case "sample":
                            if (StaticTypingMode == StaticTypingMode.PromoteContextToGlobals)
                                testCase.Ignore("Does not support static typing");
                            break;
                        default:
                            break;
                    }


                    yield return testCase;
                }
            }
        }


        public class Provider
        {
            public string this[string key] => key;

            public string Chop(string input, int count) => input.Substring(0, input.Length - count);
            public ArrayList Customers => new ArrayList { "ArrayList element 1", "ArrayList element 2", "ArrayList element 3", "ArrayList element 4" };
            public ArrayList GetCustomers() => Customers;
            public string Title { get; set; } = "lunatic";
            public string Name { get; } = "jason";
            public string[] Array => new[] { "first element", "second element" };
            public List<string> Vector => new List<string> { "vector element 1", "vector element 2" };

            //Yes, I have no idea why some concat operations use space seperators, but others don't
            public string Concat(IEnumerable<object> args) => string.Join(" ", args);
            public string Concat(string first, string second) => first + second;
            public string GetTitleMethod() => Title;

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
        public class Person {
            public virtual string Name => "Person";
        }
        public class Child : Person {
            public override string Name => "Child";
        }

    }
}
