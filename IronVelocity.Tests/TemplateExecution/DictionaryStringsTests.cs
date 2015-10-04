using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

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

        [TestCase("%{key='value'}")]
        [TestCase("%{  key='value'}")]
        [TestCase("%{key   ='value'}")]
        [TestCase("%{  key ='value'}")]
        [TestCase("%{key='value'}")]
        [TestCase("%{key=  'value'}")]
        [TestCase("%{key='value'  }")]
        [TestCase("%{key=  'value'  }")]
        [TestCase("%{  key  =  'value'  }")]
        public void ShouldProcessDictionaryWithWhitespaceAroundKeysOrValuesWithConstantValues(string dictString)
        {
            var script = "#set($dict = \"" + dictString + "\")";

            var execution = ExecuteTemplate(script);

            Assert.That(execution.Context.Keys, Contains.Item("dict"));
            Assert.That(execution.Context["dict"], Is.InstanceOf<IDictionary>());

            var dict = (IDictionary)execution.Context["dict"];
            Assert.That(dict.Keys, Contains.Item("key"));
            Assert.That(dict["key"], Is.EqualTo("value"));

        }

        [TestCase("'hello world'", "hello world")]
        [TestCase("'hello $x world'", "hello beautiful world")]
        [TestCase("'$i'", (int)72)] //This doesn't make sense to me, but is what NVelocity does, so maintain for backwards compatability.
        [TestCase("'$i$i'", "7272")]
        [TestCase("97", (int)97)]
        [TestCase("44.67", (float)44.67)]
        [TestCase("$x", "beautiful")]
        public void ShouldProcessDictionaryValue(string input, object expected)
        {
            var script = "#set($dict = \"%{ key = " + input + "}\")";
            var env = new
            {
                i = 72,
                x = "beautiful",
            };

            var execution = ExecuteTemplate(script, locals: env);

            Assert.That(execution.Context.Keys, Contains.Item("dict"));
            var result = execution.Context["dict"];
            Assert.That(result, Is.InstanceOf<IDictionary>());

            var dict = result as IDictionary;
            Assert.That(dict.Keys, Contains.Item("key"));
            Assert.That(dict["key"], Is.EqualTo(expected));
        }
    }
}
