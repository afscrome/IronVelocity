﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    using Parser = IronVelocity.Parser.Parser;
    using IronVelocity.Parser.AST;
    [TestFixture]
    public class ArgumentListTests
    {
        [TestCase("($door)")]
        [TestCase("( $door)")]
        [TestCase("($door )")]
        [TestCase("( $door )")]
        [TestCase("(	$door)")]
        public void ParseArgumentsWithWhitspace(string input)
        {
            var parser = new Parser(input);

            var result = parser.Arguments();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Arguments, Is.Not.Null);
            Assert.That(result.Arguments.Count == 1);

            var arg = result.Arguments.Single();
            Assert.That(arg, Is.TypeOf<ReferenceNode>());
            var reference = (ReferenceNode)arg;
            Assert.That(reference.Value, Is.TypeOf<Variable>());

            var variable = reference.Value as Variable;
            Assert.That(variable.Name, Is.EqualTo("door"));
        }

        [TestCase("()")]
        [TestCase("( )")]
        [TestCase("(\t)")]
        public void ParseEmptyArgumentList(string input)
        {
            var parser = new Parser(input);
            var result = parser.Arguments();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Arguments, Is.Not.Null);
            Assert.That(result.Arguments.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParseMultipleArguments()
        {
            var input = "($cat, $mat)";
            var parser = new Parser(input);

            var result = parser.Arguments();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Arguments, Is.Not.Null);
            Assert.That(result.Arguments.Count, Is.EqualTo(2));

            var firstArg = result.Arguments[0];
            Assert.That(firstArg, Is.TypeOf<ReferenceNode>());
            var reference = (ReferenceNode)firstArg;
            Assert.That(reference.Value, Is.TypeOf<Variable>());

            var variable = reference.Value as Variable;
            Assert.That(variable.Name, Is.EqualTo("cat"));

            var secondArg = result.Arguments[1];
            Assert.That(firstArg, Is.TypeOf<ReferenceNode>());
            var secondReference = (ReferenceNode)secondArg;
            Assert.That(secondReference.Value, Is.TypeOf<Variable>());

            var secondVariable = secondReference.Value as Variable;
            Assert.That(secondVariable.Name, Is.EqualTo("mat"));
        }
    }
}