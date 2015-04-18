using IronVelocity.Runtime;
using NUnit.Framework;
using NVelocity.Test.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tests;

namespace IronVelocity.Tests.Regression
{
    [TestFixture]
    public class RegressionTests
    {
        private static readonly string _base = "..\\..\\Regression\\templates\\";
        private static readonly string _failureResultsDir = "Failures";

        private IDictionary<string, object> _environment;

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

        [SetUp]
        public void SetUp()
        {
            _environment = new Dictionary<string, object>();
            var context = _environment;


            var provider = new TestProvider();
            var al = provider.Customers;
            var h = new RuntimeDictionary();

            h.Add("Bar", "this is from a hashtable!");
            h.Add("Foo", "this is from a hashtable too!");

            /*
            *  lets set up a vector of objects to test late introspection. See ASTMethod.java
            */

            var vec = new ArrayList();

            vec.Add(new String("string1".ToCharArray()));
            vec.Add(new String("string2".ToCharArray()));

            /*
            *  set up 3 chained contexts, and add our data 
            *  through the 3 of them.
            */

            context["provider"] = provider;
            context["name"] = "jason";
            context["providers"] = provider.Customers2;
            context["list"] = al;
            context["hashtable"] = h;
            context["hashmap"] = new RuntimeDictionary();
            context["search"] = provider.Search;
            context["relatedSearches"] = provider.RelSearches;
            context["searchResults"] = provider.RelSearches;
            context["stringarray"] = provider.Array;
            context["vector"] = vec;
            context["mystring"] = new String(string.Empty.ToCharArray());
            //context["runtime"] =new FieldMethodizer("NVelocity.Runtime.RuntimeSingleton";
            //context["fmprov"] =new FieldMethodizer(provider);
            context["Floog"] = "floogie woogie";
            context["boolobj"] = new BoolObj();

            /*
        *  we want to make sure we test all types of iterative objects
        *  in #foreach()
        */

            Object[] oarr = new Object[] { "a", "b", "c", "d" };
            int[] intarr = new int[] { 10, 20, 30, 40, 50 };

            context["collection"] = vec;
            context["iterator"] = vec.GetEnumerator();
            context["map"] = h;
            context["obarr"] = oarr;
            context["enumerator"] = vec.GetEnumerator();
            context["intarr"] = intarr;


        }

        [Test]
        [TestCaseSource("TestsCases", Category = "Regression")]
        public void RegressionTest(string testName)
        {
            var inputFile = Path.Combine(_base, testName + ".vm");
            var expectedOutputFile = Path.Combine(_base, "Expected", testName + ".cmp");

            if (!File.Exists(inputFile))
                Assert.Fail("Regression Template '{0}' does not exist", inputFile);

            if (!File.Exists(expectedOutputFile))
                Assert.Fail("Expected result for Regression Template '{0}' does not exist", expectedOutputFile);

            var input = File.ReadAllText(inputFile);
            var expectedOutput = File.ReadAllText(expectedOutputFile);

            var output = Utility.GetNormalisedOutput(input, _environment, inputFile);
            expectedOutput = Utility.NormaliseLineEndings(expectedOutput);

            try
            {
                Assert.AreEqual(expectedOutput, output);
            }
            catch (AssertionException)
            {
                File.WriteAllText(Path.Combine(_failureResultsDir, testName + ".actual.txt"), output);
                File.WriteAllText(Path.Combine(_failureResultsDir, testName + ".expected.txt"), expectedOutput);
                File.WriteAllText(Path.Combine(_failureResultsDir, testName + ".input.txt"), Utility.NormaliseLineEndings(input));
                //TODO: save results for further investigation.
                throw;
            }
        }


        public static IEnumerable<TestCaseData> TestsCases
        {
            get
            {
                foreach (var file in Directory.GetFiles(_base, "*.vm"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var testCase = new TestCaseData(name)
                        .SetName("Regression Test: " + name + ".vm");

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
    }

}
