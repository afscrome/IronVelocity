using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.StaticTyping
{
    public class If
    {
        [Test]
        public void IfStatementWithStaticlyTypedCondition()
        {
            var globals = new Dictionary<string, object>();
            globals.Add("x", true);

            var input = "#if($x)yes#end";

            Utility.TestExpectedMarkupGenerated(input, "yes", globals, isGlobalEnvironment: true);
        }

    }
}
