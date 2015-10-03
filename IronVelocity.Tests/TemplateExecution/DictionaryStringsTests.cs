using NUnit.Framework;
using System.Collections;

namespace IronVelocity.Tests.TemplateExecution
{
    public class DictionaryStringsTests : TemplateExeuctionBase
    {
        [TestCase("%{}")]
        [TestCase("%{    }")]
        public void ShouldProcessEmptyDictionary(string stringContent)
        {
            var input = $"#set($result = \"{stringContent}\")";

            var execution = ExecuteTemplate(input);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            var result = execution.Context["result"];
            Assert.That(result, Is.InstanceOf<IDictionary>());
            Assert.That(result, Is.Empty);
        }
    }
}
