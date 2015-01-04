using IronVelocity.Compilation;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronVelocity.Tests
{
    [TestFixture]
    public class MethodInfoReflectionTests
    {
        public IEnumerable<TestCaseData> TestCases()
        {
            return typeof(MethodHelpers)
                .GetFields()
                .Select(x => new TestCaseData(x).SetName("MethodHelpers '" + x.Name + "' is not Null"));
        }

        [TestCaseSource("TestCases")]
        public void Test(FieldInfo field)
        {
            Assert.NotNull(field.GetValue(null));
        }

    }
}
