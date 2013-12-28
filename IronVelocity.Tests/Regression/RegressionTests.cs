using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.Regression
{
    [TestFixture]
    public class RegressionTests
    {
        private static readonly string _base = "c:\\Projects\\IronVelocity\\IronVelocity.Tests\\Regression\\templates\\";

        [Test]
        [TestCaseSource("TestsCases",Category = "Regression")]
        public void RegressionTest(string inputFile, string expectedOutputFile)
        {
            if (!File.Exists(inputFile))
                Assert.Inconclusive("File '{0}' does not exist", inputFile);

            if (!File.Exists(expectedOutputFile))
                Assert.Inconclusive("File '{0}' does not exist", expectedOutputFile);

            var input = File.ReadAllText(inputFile);
            var expectedOutput = File.ReadAllText(expectedOutputFile);

            var provider = new NVelocity.Test.Provider.TestProvider();

            var context = new Dictionary<string, object>{
                {"provider", provider}
            };

            var output = Utility.GetNormalisedOutput(input, context);
            expectedOutput = Utility.NormaliseLineEndings(expectedOutput);

            try
            {
                Assert.AreEqual(expectedOutput, output);
            }
            catch (AssertionException ex)
            {
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
                    yield return new TestCaseData(
                            file,
                            Path.Combine(_base, "Expected", name + ".cmp")
                        )
                        .SetName("Regression Test: " + name + ".vm");

                }
            }
        }
    }

}
