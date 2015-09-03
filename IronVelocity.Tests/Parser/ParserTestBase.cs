using Antlr4.Runtime;
using IronVelocity.Parser;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;


namespace IronVelocity.Tests.Parser
{
    [Category("Parser")]
    public abstract class ParserTestBase
    {
        protected VelocityParser CreateParser(string input, int? lexerMode = null)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VelocityParser(tokenStream)
            {
                ErrorHandler = new TestBailErrorStrategy(input, lexerMode)
            };

            if (lexerMode.HasValue)
                lexer.Mode(lexerMode.Value);

            return parser;
        }

        protected void ParseBinaryExpressionTest(IParseTree parsed, string input, string left, string right, int operatorTokenKind)
        {
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.GetText(), Is.EqualTo(input));
            Assert.That(parsed.ChildCount, Is.EqualTo(3));

            Assert.That(GetTerminalNodeTokenType(parsed.GetChild(1)), Is.EqualTo(operatorTokenKind));

            Assert.That(parsed.GetChild(0).GetText().Trim(), Is.EqualTo(left));
            Assert.That(parsed.GetChild(2).GetText().Trim(), Is.EqualTo(right));
        }

        protected void ParseTernaryExpressionWithEqualPrecedenceTest(IParseTree parsed, string input, int leftOperatorKind, int rightOperatorKind)
        {
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
