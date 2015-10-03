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
        public void ShouldRenderPropertyInvocation()
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
        public void ShouldRenderPropertyInvocation_WhenFirstCharacterCaseIsDifferent(string input)
        {
            var context = new Dictionary<string, object>
            {
                ["data"] = new[] {1,2,3}
            };

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo("3"));
        }

        [Test]
        public void ShouldRenderMethodCall()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = new Guid("35fae588-0bad-4ea0-b092-619e00323041")
            };
            var input = "$input.ToString()";

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo("35fae588-0bad-4ea0-b092-619e00323041"));
        }

        [Test]
        public void ShouldRenderMethodCallWithArgument()
        {
            var context = new Dictionary<string, object>
            {
                ["input"] = new Guid("35fae588-0bad-4ea0-b092-619e00323041")
            };
            var input = "$input.ToString('n')";

            var result = ExecuteTemplate(input, context);

            Assert.That(result.Output, Is.EqualTo("35fae5880bad4ea0b092619e00323041"));
        }

        [TestCase("$foo")]
        [TestCase("${formal}")]
        [TestCase("$bar.bat")]
        [TestCase("$fizz.buzz()")]
        [TestCase("$hello.world($fizzbuzz)")]
        [TestCase("$one.two( $three )")]
        public void ShouldRenderOriginalSourceForUnsilencedNullReference(string input)
        {
            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(input));
        }

        [TestCase("$!foo")]
        [TestCase("$!{formal}")]
        [TestCase("$!bar.bat")]
        [TestCase("$!fizz.buzz()")]
        [TestCase("$!hello.world($fizzbuzz)")]
        [TestCase("$!one.two( $three )")]
        public void ShouldRenderNothingForSilencedNullReference(string input)
        {
            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.Empty);
        }
    }
}
