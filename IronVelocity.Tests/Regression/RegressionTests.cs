using NUnit.Framework;
using System;
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
        string _base = "c:\\Projects\\IronVelocity\\IronVelocity.Tests\\Regression\\templates\\";
        [TestCase("arithmetic")]
        [TestCase("array")]
        [TestCase("block")]
        [TestCase("comment")]
        [TestCase("equality")]
        [TestCase("escape")]
        [TestCase("foreach-array")]
        [TestCase("foreach-method")]
        [TestCase("foreach-variable")]
        [TestCase("formal")]
        [TestCase("if")]
        public void RegressionTest(string testName)
        {
            var input = File.ReadAllText(Path.Combine(_base, testName + ".vm"));
            var expectedOutput = File.ReadAllText(Path.Combine(_base, "expected", testName + ".cmp"));

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
            catch(AssertionException ex)
            {
                if (!Directory.Exists("ActualResults"))
                    Directory.CreateDirectory("ActualResults");

                File.WriteAllText(Path.Combine("ActualResults", testName + ".txt"), output);
                throw;
            }
        }
    }
}
