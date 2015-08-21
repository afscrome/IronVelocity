﻿using IronVelocity.Parser;
using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class BooleanTests : ParserTestBase
    {
        [TestCase("true")]
        [TestCase("false")]
        public void BooleanParseTests(string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).boolean();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));
        }
    }
}