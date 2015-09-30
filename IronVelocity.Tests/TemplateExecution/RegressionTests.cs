using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace IronVelocity.Tests.TemplateExecution
{
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

        [TestCaseSource(nameof(TestsCases)), Explicit]
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
                var failurePath = Path.Combine(_failureResultsDir, name);
                File.WriteAllText(failurePath + ".expected", expected);
                File.WriteAllText(failurePath + ".actual", result.OutputWithNormalisedLineEndings);
                File.WriteAllText(failurePath + ".input", input);

                throw;
            }
        }

        private IDictionary<string, object> CreateContext(string name)
        {
            var context = new Dictionary<string, object>
            {
                ["provider"] = new Provider()
            };

            switch (name)
            {
                case "diabolical":
                    context["stringarray"] = new[] { "first element", "second element" };
                    break;
            }

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
            public string Chop(string input, int count) => input.Substring(0, input.Length - count);
        }
    }
}
