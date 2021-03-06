﻿using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class BadInput : ParserTestBase
    {

        //If we start a formal reference, but don't' finish it, can't fallback to text
        //[TestCase("${")]
        //[TestCase("$${")]
        [TestCase("${foo")]
        [TestCase("${foo ")]
        [TestCase("${foo .")]
        //TODO: investigate behavior around when un-closed parenthesis is allowed at the end of a reference
        // In NVelocity, the following are treated as text: "$foo.(", "$foo.(   ", "$foo.(."
        // But the following are errors "$foo.(123", "$foo.(123"
        [TestCase("$foo.bar( ")]
        [TestCase("$test.bar(,)")]
        [TestCase("$foo.bar(")]
        [TestCase("$foo.bar(.")]
        [TestCase("$foo.bar(hello")]
        //Incomplete block comments
        [TestCase("#*")]
        [TestCase("#* #* *#")]
        public void ShouldProduceParserError(string input)
        {
            ParseShouldProduceError(input, x => x.template());
        }

        [Test]
        public void UnexpectedEndProvidesHelpfulError()
        {
            var input = "#end";
            var exception = ParseShouldProduceError(input, x => x.template());

            Assert.That(exception.Message, Contains.Substring("Unexpected #end"));
        }
    }
}
