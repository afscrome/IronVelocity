using Antlr4.Runtime;
using IronVelocity.Parser;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;
using System;
using Antlr4.Runtime.Misc;
using System.Diagnostics;

namespace IronVelocity.Tests.Parser
{
    [Category("Parser")]
    public abstract class ParserTestBase
    {

        protected T Parse<T>(string input, Func<VelocityParser, T> parseFunc, int? lexerMode = null)
            where T : RuleContext
        {
            return new AntlrVelocityParser().ParseTemplate(input, Utility.GetName(), parseFunc, lexerMode);
        }


        protected void ParseShouldProduceError(string input, Func<VelocityParser, RuleContext> parseFunc, int? lexerMode = null, bool shouldLexCompletely = true)
        {
            RuleContext parsed;
            try
            {
                parsed = Parse(input, parseFunc, lexerMode);
            }
            catch(ParseCanceledException)
            {
                Assert.Pass();
                throw;
            }

            //For debugging purposes, print the tokens & parse tree generated

            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            if (lexerMode.HasValue)
                lexer.Mode(lexerMode.Value);

            foreach (var token in lexer.GetAllTokens())
            {
                Console.WriteLine(token);
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(token);
                }
            }

            Console.WriteLine(parsed.ToStringTree(new VelocityParser(null)));
            Assert.Fail("No Parse Errors Occurred;");
        }

        protected void ParseBinaryExpressionTest(string input, string left, string right, int operatorTokenKind, Func<VelocityParser, RuleContext> parseFunc)
        {
            var parsed = Parse(input, parseFunc, VelocityLexer.ARGUMENTS);
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.GetText(), Is.EqualTo(input));
            Assert.That(parsed.ChildCount, Is.EqualTo(3));

            Assert.That(GetTerminalNodeTokenType(parsed.GetChild(1)), Is.EqualTo(operatorTokenKind));

            Assert.That(parsed.GetChild(0).GetText().Trim(), Is.EqualTo(left));
            Assert.That(parsed.GetChild(2).GetText().Trim(), Is.EqualTo(right));
        }


        protected void ParseTernaryExpressionWithEqualPrecedenceTest(string input, int leftOperatorKind, int rightOperatorKind, Func<VelocityParser, RuleContext> parseFunc)
        {
            var parsed = Parse(input, parseFunc, VelocityLexer.ARGUMENTS);
            Assert.That(parsed, Is.Not.Null);

            Assert.That(parsed.GetText(), Is.EqualTo(input));
            Assert.That(parsed.ChildCount, Is.EqualTo(3));
            Assert.That(parsed.GetChild(0).ChildCount, Is.EqualTo(3));

            Assert.That(parsed.GetChild(0).GetChild(0).GetText().Trim(), Is.EqualTo("$a"));
            Assert.That(GetTerminalNodeTokenType(parsed.GetChild(0).GetChild(1)), Is.EqualTo(leftOperatorKind));
            Assert.That(parsed.GetChild(0).GetChild(2).GetText().Trim(), Is.EqualTo("$b"));
            Assert.That(GetTerminalNodeTokenType(parsed.GetChild(1)), Is.EqualTo(rightOperatorKind));
            Assert.That(parsed.GetChild(2).GetText().Trim(), Is.EqualTo("$c"));
        }

        protected int? GetTerminalNodeTokenType(IParseTree node)
        {
            var terminalNode = (node as ITerminalNode);
            return terminalNode?.Symbol.Type;
        }

    }
}
