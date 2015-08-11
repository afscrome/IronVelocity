using Antlr4.Runtime;
using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;

namespace IronVelocity.Tests.Parser
{


    public class ParserTest
    {
        [TestCase("Basic Test")]
        [TestCase("$salutation $name")]
        public void BasicTest(string input)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VelocityParser(tokenStream);

            var parsed = parser.template();


            var visitor = new TestVisitor();
            visitor.Visit(parsed);

            if (visitor.Errors.Any())
                Assert.Fail($"{visitor.Errors.Count} errors occurred;");
        }


        [TestCase("$")]
        [TestCase("$$")]
        [TestCase("$!")]
        [TestCase("$!!")]
        [TestCase("$!!{")]
        public void ReferenceLikeTextThatShouldBeTreatedAsText(string input)
        {
            BasicTest(input);
        }

        [TestCase("$foo")]
        [TestCase("$!bar")]
        [TestCase("${foo}")]
        [TestCase("$!{bar}")]
        public void BasicVariableReferences(string input)
        {
            BasicTest(input);
        }

        [TestCase("Hello $name")]
        [TestCase("$color Box")]
        [TestCase("$$foo")]
        [TestCase("$!$!!foo")]
        public void MixedTextAndVariableReferences(string input)
        {
            BasicTest(input);
        }


        [TestCase("${")]
        [TestCase("$${")]
        [TestCase("${foo")]
        public void ShouldProduceParserError(string input)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VelocityParser(tokenStream);

            var parsed = parser.template();

            var visitor = new TestVisitor();
            visitor.Visit(parsed);

            if (visitor.Errors.Any())
                Assert.Fail($"No Parse Errors Occurred;");
        }

        //TODO: investigate behaviour around when un-closed paranthesis is allowed at the end of a reference
        // In NVelocity, the followign are treated as text: "$foo.(", "$foo.(   ", "$foo.(."
        // But the followign are errors "$foo.(123", "$foo.(123"


        public class TestVisitor : VelocityParserBaseVisitor<object>
        {
            public ICollection<IErrorNode> Errors { get; } = new List<IErrorNode>();

            public override object VisitErrorNode(IErrorNode node)
            {
                Errors.Add(node);
                return null;
            }
        }


        /*
        var lexerErrorListener = new LexerErrorListener();
        var parserErrorListener = new ParserErrorListener();

        lexer.ErrorListeners.Clear();
        parser.ErrorListeners.Clear();
        lexer.ErrorListeners.Add(lexerErrorListener);
        parser.ErrorListeners.Add(parserErrorListener);

        if (lexerErrorListener.ErrorCount > 0 || parserErrorListener.ErrorCount > 0)
            Assert.Fail($"{lexerErrorListener.ErrorCount} lexer errors occurred, {parserErrorListener.ErrorCount} parser errors occurred.");

        */

        private class LexerErrorListener : ConsoleErrorListener<int>
        {
            public int ErrorCount { get; private set; }

            public override void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                ErrorCount++;
                base.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            }
        }

        private class ParserErrorListener : ConsoleErrorListener<IToken>, IParserErrorListener
        {
            public int ErrorCount { get; private set; }

            public void ReportAmbiguity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
            {
                ErrorCount++;
            }

            public void ReportAttemptingFullContext(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
            {
                ErrorCount++;
            }

            public void ReportContextSensitivity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
            {
                ErrorCount++;
            }

            public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                ErrorCount++;
                base.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            }
        }



    }
}
