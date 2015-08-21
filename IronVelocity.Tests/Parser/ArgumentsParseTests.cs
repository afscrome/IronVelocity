using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class ArgumentListTests : ParserTestBase
    {
        [TestCase(0, "")]
        [TestCase(0, " ")]
        [TestCase(0, "\t")]
        [TestCase(0, "\t \t\t  ")]
        [TestCase(1, "1")]
        [TestCase(1, "  1")]
        [TestCase(1,"1  ")]
        [TestCase(1, "  1   ")]
        [TestCase(3, "1,1,1")]
        [TestCase(3, " 1 , 1 , 1 ")]
        public void ArgumentListParseTest(int argumentCount, string input)
        {
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).argument_list();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            var arguments = result.GetRuleContexts<VelocityParser.ArgumentContext>();
            Assert.That(arguments.Count, Is.EqualTo(argumentCount));
        }

        [TestCase(",")]
        [TestCase(",,")]
        [TestCase("1,,1")]
        [TestCase("1,")]
        [TestCase(",1")]
        public void InvalidArgumentListParseTest(string input)
        {
            var parser = CreateParser(input, VelocityLexer.ARGUMENTS);

            IParseTree parsed;
            try
            {
                parsed = parser.argument_list();
            }
            catch (ParseCanceledException)
            {
                Assert.Pass();
                throw;
            }

            if (parsed.GetText() == input)
            {
                TestBailErrorStrategy.PrintTokens(input);
                Console.WriteLine(parsed.ToStringTree(parser));
            }

        }
    }
}
