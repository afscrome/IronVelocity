using IronVelocity.Compilation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    [TestFixture]
    public class Globals
    {
        [Test]
        public void Test2()
        {
            var sampleGlobal = new SampleGlobal();
            var globals = new Dictionary<string, object>() {
                {"sampleGlobal", sampleGlobal}
            };

            Test("$sampleGlobal.Property", "IronVelocity.Tests.OutputTests.Globals+SampleGlobal", globals);
        }

        private void Test(string input, string expectedoutput, IDictionary<string, object> globals)
        {
            var runtime = new VelocityRuntime(null, globals);

            var originalTree = runtime.GetExpressionTree(input, "test");

            var typeMap = globals.ToDictionary(x => x.Key, x => x.Value.GetType());

            var staticTypeVisitor = new StaticGlobalVisitor(typeMap);
            var reducedTree = staticTypeVisitor.Visit(originalTree);

            throw new NotImplementedException();

        }

        public class SampleGlobal
        {
            public int Property { get { return 123; } }

            public string NoParams()
            {
                return "No Params";
            }
        }
    }
}
