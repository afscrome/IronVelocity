using NUnit.Framework;
using System;

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
        [TestCase("$foo.bar(", Ignore = true, IgnoreReason = "Determine whether this should or shouldn't be a parse error")]
        [TestCase("$foo.bar(.", Ignore = true, IgnoreReason = "Determine whether this should or shouldn't be a parse error")]
        [TestCase("$foo.bar(hello", Ignore = true, IgnoreReason = "Determine whether this should or shouldn't be a parse error")]
        //Incomplete block comments
        [TestCase("#*")]
        [TestCase("#* #* *#")]
        public void ShouldProduceParserError(string input)
        {
            var parser = CreateParser(input);

            var parsed = parser.template();

            if (parser.NumberOfSyntaxErrors == 0)
            {
                PrintTokens(input);
                Console.WriteLine(parsed.ToStringTree(parser.TokenNames));
                Assert.Fail($"No Parse Errors Occurred;");
            }
        }
    }
}
