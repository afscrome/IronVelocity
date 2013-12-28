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
            var expectedOutput = File.ReadAllText(Path.Combine(_base, "results", testName + ".res"));

            Utility.TestExpectedMarkupGenerated(input, expectedOutput);
        }
    }
}
