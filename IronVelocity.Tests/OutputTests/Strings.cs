using NUnit.Framework;
using System;
using System.Collections.Generic;
using IronVelocity.Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Strings
    {
        //Key whitespace
        [TestCase("%{key='&&&'}")]
        [TestCase("%{  key='&&&'}")]
        [TestCase("%{key   ='&&&'}")]
        [TestCase("%{  key ='&&&'}")]
        //Value whitespace
        [TestCase("%{key='&&&'}")]
        [TestCase("%{key=  '&&&'}")]
        [TestCase("%{key='&&&'  }")]
        [TestCase("%{key=  '&&&'  }")]
        //NonEx
        public void DictionaryTest_with_KeyValueWhitespace(string dictTemplate)
        {
            foreach (var keyValue in new[] { "value",  "  me", "you  ", "   us  " })
            {
                var dictString = dictTemplate.Replace("&&&", keyValue);
                var script = "#set($dict = \"" + dictString + "\")".Replace("&&&", dictString);

                var env = new VelocityContext();
                Utility.GetNormalisedOutput(script, env);

                CollectionAssert.Contains(env.Keys, "dict");

                var dict = (IDictionary<object, object>)env["dict"];
                CollectionAssert.Contains(dict.Keys, "key");

                Assert.AreEqual(keyValue.Trim(), dict["key"]);
            }

        }


        [TestCase("%{key=$val}")]
        [TestCase("%{key=  $val}")]
        [TestCase("%{key=$val   }")]
        [TestCase("%{key=  $val   }")]
        [TestCase("%{   key=$val}")]
        [TestCase("%{  key=  $val}")]
        [TestCase("%{ key=$val   }")]
        [TestCase("%{  key=  $val   }")]
        [TestCase("%{key   =$val}")]
        [TestCase("%{key  =  $val}")]
        [TestCase("%{key  =$val   }")]
        [TestCase("%{key =  $val   }")]
        [TestCase("%{  key =$val}")]
        [TestCase("%{  key   =  $val}")]
        [TestCase("%{  key =$val   }")]
        [TestCase("%{  key  =  $val   }")]
        public void DictionaryTest_with_Variable(string dictString)
        {
            var values = new object[] { 123, new List<string>{"hello"}};

            foreach (var value in values)
            {
                var script = "#set($dict = \"" + dictString + "\")";

                var env = new VelocityContext();
                env.Add("val", value);
                Utility.GetNormalisedOutput(script, env);

                CollectionAssert.Contains(env.Keys, "dict");

                var dict = (IDictionary<object, object>)env["dict"];
                CollectionAssert.Contains(dict.Keys, "key");

                Assert.AreEqual(value, dict["key"]);
            }

        }

        [TestCase("'hello world'", "hello world")]
        [TestCase("'hello $x world'", "hello beautiful world")]
        [TestCase("'$i'", 72)]
        [TestCase("'$i$i'", "7272")]
        [TestCase("'$i$i'", "7272")]
        [TestCase("97", (int)97)]
        [TestCase("44.67", (float)44.67)]
        public void DictionaryTest_with_Constant(string input, object expected)
        {
            var env = new VelocityContext();
            env["i"] = 72;
            env["x"] = "beautiful";

            var script = "#set($dict = \"%{ key = " + input + "}\")";

            Utility.GetNormalisedOutput(script, env);

            var dict = (IDictionary<object, object>)env["dict"];
            CollectionAssert.Contains(dict.Keys, "key");

            var value = dict["key"];
            Assert.IsInstanceOf(expected.GetType(), value);
            Assert.AreEqual(expected, value);
        }

    }
}
