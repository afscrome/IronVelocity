using Antlr4.Runtime;
using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Atn;
using System.Diagnostics;

namespace IronVelocity.Tests.Parser
{
    [Category("Parser")]
    public abstract class ParserTestBase
    {
        /*
        [TestCase("$firstname", " ", "$lastname")]
        [TestCase("$greeting", ", ", "$name")]
        [TestCase("${length}", "x", "${width}")]
        [TestCase("$dollars", "$", "$cents")]
        [TestCase("$mama", "!", "$mia")]
        [TestCase("$go", "}", "$again")]
        [TestCase("$pounds", ".", "$pennies")]
        [TestCase("$user.firstname", " ", "$user.lastname")]
        [TestCase("$welcome.greeting", ", ", "$welcome.name")]
        [TestCase("${item.length}", "x", "${item.width}")]
        [TestCase("$cost.pounds", ".", "$cost.pennies")]
        [TestCase("$cost.dollars", "$", "$cost.cents")]
        public void TwoReferenceswithTextInBetween(string reference1, string text, string reference2)
        {
            var input = reference1 + text + reference2;
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            Assert.That(textNode.GetText(), Is.EqualTo(text));

            var references = flattened.OfType<VelocityParser.ReferenceContext>()
                .Select(x => x.GetText())
                .ToList();

            Assert.That(references, Contains.Item(reference1));
            Assert.That(references, Contains.Item(reference2));
        }



    */




        protected VelocityParser CreateParser(string input)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VelocityParser(tokenStream);

            var lexerErrorListener = new LexerErrorListener();
            var parserErrorListener = new ParserErrorListener();

            lexer.ErrorListeners.Clear();
            parser.ErrorListeners.Clear();
            lexer.ErrorListeners.Add(lexerErrorListener);
            parser.ErrorListeners.Add(parserErrorListener);

            return parser;
        }

        protected void PrintTokens(string input)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);

            foreach (var token in lexer.GetAllTokens())
            {
                Console.WriteLine(token);
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(token);
                }
            }

        }

        protected IParseTree ParseEnsuringNoErrors(string input)
        {
            var parser = CreateParser(input);


            var parsed = parser.template();

            if (parser.NumberOfSyntaxErrors > 0)
            {
                PrintTokens(input);
                Console.WriteLine(parsed.ToStringTree(parser.TokenNames));
                Assert.Fail($"{parser.NumberOfSyntaxErrors} errors occurred;");
            }
            else
            {
                //Console.WriteLine(parsed.ToStringTree(parser.TokenNames));
            }

            return parsed;
        }


        protected IEnumerable<IParseTree> FlattenParseTree(IParseTree parseTree)
        {
            Stack<IParseTree> nodes = new Stack<IParseTree>();
            nodes.Push(parseTree);

            do
            {
                var node = nodes.Pop();
                yield return node;
                for (int i = 0; i < node.ChildCount; i++)
                {
                    nodes.Push(node.GetChild(i));
                }
            }
            while (nodes.Any());
        }


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
                Console.WriteLine("Ambiguity!");
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
