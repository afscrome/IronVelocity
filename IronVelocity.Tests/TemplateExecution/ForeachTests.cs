using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.TemplateExecution
{
    public class ForeachTests : TemplateExeuctionBase
    {
        [Test]
        public void Test()
        {
            var input = "#foreach($x in [1..2])$x,#end";
            var expected = "1,2,";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));

        }
    }
}
