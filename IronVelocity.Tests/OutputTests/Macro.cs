using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.OutputTests
{
    [TestFixture]
    public class Macro
    {
        [Test]
        public void MacroWithNoArguments()
        {
            var input = "#macro(noArgs)Hello#end #noArgs()";
            var expected = " Hello";
            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [TestCase("'test'", "test", TestName="MacroWithStringArgument")]
        [TestCase("123", 123, TestName = "MacroWithIntegerArgument")]
        [TestCase("3.14", 3.14, TestName = "MacroWithFloatArgument", Ignore=true, Description="Is this disallowed by velocity, or an NVelocity parser specific issue")]
        [TestCase("true", true, TestName = "MacroWithBooleanArgument")]
        [TestCase("'837'", 837, TestName = "MacroWithQoutedIntegerArgument")]
        [TestCase("'7.824'", 7.824, TestName = "MacroWithQuotedFloatArgument")]
        public void MacroWithConstantArgument(string inputSubstitution, object expectedSubstitution)
        {
            var input = String.Format("#macro(oneArg $val)$val|$val#end #oneArg({0})", inputSubstitution);
            var expected = String.Format(" {0}|{0}", expectedSubstitution);
            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void MacroWithTwoArguments()
        {
            var input = "#macro(twoArgs $val1 $val2)$val2+$val1#end #twoArgs('test' 123)";
            var expected = " 123+test";
            Utility.TestExpectedMarkupGenerated(input, expected);
        }


        [Test]
        public void MacroWithTwoArguments2()
        {
            var input = "#macro(outer $a)$a#end#macro(inner $b)#outer($b)#end#foreach($i in [1..3])#outer($i)#end";
            var expected = "123";
            Utility.TestExpectedMarkupGenerated(input, expected);
        }

        [Test]
        public void MacroInInterpolatedStringWhenMacroDefinedOutsideOfString()
        {
            var input = "#macro(test)Boo#end#set($result = \"#test()\")$result";
            var expected = "Boo";

            var context = new VelocityContext();

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);

            //TODO: This is really more than an output test as we're testing the internal evaluation.
            Assert.That(context.Keys, Contains.Item("result"));
            Assert.That(context["result"], Is.EqualTo(expected));
        }

        [Test]
        public void MacroInInterpolatedStringWhenMacroDefinedOutsideOfStringWithArguments()
        {
            //Failing due to the Parser used in InterpoatedString not knowing about macro from outer scope.

            var input = "#macro(hello $name)Hello $name#end#set($result = \"#hello('Bob')\")$result";
            var expected = "Hello Bob";

            var context = new VelocityContext();

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);

            //TODO: This is really more than an output test as we're testing the internal evaluation.
            Assert.That(context.Keys, Contains.Item("result"));
            Assert.That(context["result"], Is.EqualTo(expected));
        }    
    }
}
