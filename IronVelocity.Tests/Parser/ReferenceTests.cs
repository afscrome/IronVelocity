using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Parser;
using NUnit.Framework;
using IronVelocity.Parser.AST;

namespace IronVelocity.Tests.Parser
{
    using Parser = IronVelocity.Parser.Parser;

    [TestFixture]
    public class ReferenceTests
    {
        [TestCase("$foo", false, false, "foo")]
        [TestCase("$!bar", true, false, "bar")]
        [TestCase("${baz}", false, true, "baz")]
        [TestCase("$!{foobar}", true, true, "foobar")]
        public void ParseVariable(string input, bool isSilent, bool isFormal, string variableName)
        {
            var parser = new Parser(input);

            var result = parser.Reference();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSilent, Is.EqualTo(isSilent));
            Assert.That(result.IsFormal, Is.EqualTo(isFormal));

            Assert.That(result.Value, Is.TypeOf<Variable>());
            var variable = (Variable)result.Value;
            Assert.That(variable.Name, Is.EqualTo(variableName));
        }


        [TestCase("$foo.boo", false, false, "foo", "boo")]
        [TestCase("$!bar.Koopa", true, false, "bar", "Koopa")]
        [TestCase("${baz.BOB-BOMB}", false, true, "baz", "BOB-BOMB")]
        [TestCase("$!{foobar.BlOoPeR}", true, true, "foobar", "BlOoPeR")]
        public void ParseProperty(string input, bool isSilent, bool isFormal, string variableName, string propertyName)
        {
            var parser = new Parser(input);

            var result = parser.Reference();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSilent, Is.EqualTo(isSilent));
            Assert.That(result.IsFormal, Is.EqualTo(isFormal));

            Assert.That(result.Value, Is.TypeOf<Property>());
            var property = (Property)result.Value;
            Assert.That(property.Name, Is.EqualTo(propertyName));

            Assert.That(property.Target, Is.TypeOf<Variable>());
            var variable = (Variable)property.Target;
            Assert.That(variable.Name, Is.EqualTo(variableName));

        }

        [TestCase("$foo.red()", false, false, "foo", "red")]
        [TestCase("$!bar.Yellow()", true, false, "bar", "Yellow")]
        [TestCase("${baz.PINKY_BROWN()}", false, true, "baz", "PINKY_BROWN")]
        [TestCase("$!{foobar.ScArLEt()}", true, true, "foobar", "ScArLEt")]
        public void ParseMethodWithNoArguments(string input, bool isSilent, bool isFormal, string variableName, string methodName)
        {
            var parser = new Parser(input);

            var result = parser.Reference();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSilent, Is.EqualTo(isSilent));
            Assert.That(result.IsFormal, Is.EqualTo(isFormal));

            Assert.That(result.Value, Is.TypeOf<Method>());
            var method = (Method)result.Value;
            Assert.That(method.Name, Is.EqualTo(methodName));

            Assert.That(method.Target, Is.TypeOf<Variable>());
            var variable = (Variable)method.Target;
            Assert.That(variable.Name, Is.EqualTo(variableName));

        }

        /* TODO: handle invalid references - Exception? Treat as Text?
         * $
         * $$
         * ${
         * ${}
         * ${stuff
         * $!{
        */
    }
}
