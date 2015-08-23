using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.TemplateExecution
{
    public class ReferenceTests : TemplateExeuctionBase
    {
        [Test]
        public void When_RenderingPropertyInvocation_ShouldOutputPropertyValue()
        {
            var context = new Dictionary<string, object>
            {
                ["string"] = "hello World"
            };

            var input = "$string.Length";

            var result = ExecuteTemplate(input,context);

            Assert.That(result.Output, Is.EqualTo("11"));            
        }

        [TestCase("$data.Count")]
        [TestCase("$data.count")]
        public void When_RenderingPropertyInvocation_ShouldBeCaseInsensitiveForFirstCharacter(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["data"] = new[] {1,2,3}
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo("3"));
        }
    }
}
