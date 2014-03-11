using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Foreach
    {
        [Test]
        public void RendersNoDataForInvalidEnumerable()
        {
            var input = @"#set($y = false)
#foreach($x in $y)
hello
#noData
Nada
#end";
            var expected = "Nada\r\n";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void RendersNoDataForNullEnumerable()
        {
            var input = @"#foreach($x in $null)
hello
#noData
Nada
#end";
            var expected = "Nada\r\n";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }
    }
}
