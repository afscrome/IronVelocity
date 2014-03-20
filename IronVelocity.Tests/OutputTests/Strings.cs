using NUnit.Framework;
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
        public void JQueryIdSelectorWithSubstitution()
        {
            var context = new Dictionary<string, object>(){
                {"x", "myId"}
            };

            var input = "jQuery('#$x')";
            var expected = "jQuery('#myId')";

            Utility.TestExpectedMarkupGenerated(input, expected, context);
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
        public void DictionaryTest_with_KeyWhitespace(string dictTemplate)
        {
            //Velocity dictionaries are slightly screwed up in that they ignore leading & trailing whitespace in a key
            var values = new[] { "value" };//, "  me", "you  ", "   us  " };

            foreach (var value in values)
            {
                var dictString = dictTemplate.Replace("&&&", value);
                var script = "#set($dict = \"" + dictString + "\")".Replace("&&&", dictString);

                var env = new VelocityContext();
                Utility.GetNormalisedOutput(script, env);

                CollectionAssert.Contains(env.Keys, "dict");

                var dict = (IDictionary<object, object>)env["dict"];
                CollectionAssert.Contains(dict.Keys, "key");

                Assert.AreEqual(value, dict["key"]);
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


    }
}
