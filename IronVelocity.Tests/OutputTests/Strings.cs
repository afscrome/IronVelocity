using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    public class Strings
    {
        [Test]
        public void JQueryIdSelector()
        {
            var input = "jQuery('#$x')";
            var expected = "jQuery('#$x')";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void JQueryIdSelector2()
        {
            var context = new Dictionary<string, object>(){
                {"x", "myId"}
            };

            var input = "jQuery('#$x')";
            var expected = "jQuery('#myId')";

            Utility.TestExpectedMarkupGenerated(input, expected, context);
        }

        [Test]
        public void InterpolatedStringWithIfDirective()
        {
            var input = "#if(true)Hello#end";
            var expected = "Hello";

            InterpolatedStringTest(input, expected);
        }

        [Test]
        public void InterpolatedStringWithNestedIfDirective()
        {
            var input = "#if(true)#if(false) Hello #else World #end#end";
            var expected = " World ";

            InterpolatedStringTest(input, expected);
        }

        [Test]
        public void InterpolatedStringWithForeach()
        {
            var input = "#foreach($y in '1234') $y #end";
            var expected = " 1  2  3  4 ";

            InterpolatedStringTest(input, expected);
        }


        [Test]
        public void InterpolatedStringWithReference()
        {
            var input = "$value";
            var expected = "1234";
            var context = new Dictionary<string, object>
            {
                {"value", 1234}
            };

            InterpolatedStringTest(input, expected, context);
        }

        [Test]
        public void InterpolatedWithNonExistantReference()
        {
            var input = "$value";
            var expected = "$value";

            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        private void InterpolatedStringTest(string inputString, string expectedResult, IDictionary<string,object> context = null)
        {
            var inputCode = String.Format("#set($result = \"{0}\")$result", inputString);
            context = context == null
                ? new VelocityContext()
                : new VelocityContext(context);

            Utility.TestExpectedMarkupGenerated(inputCode, expectedResult, context, isGlobalEnvironment: false);

            //TODO: This is really more than an output test as we're testing the internal evaluation.
            Assert.That(context.Keys, Contains.Item("result"));
            Assert.That(context["result"], Is.EqualTo(expectedResult));
        }

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
