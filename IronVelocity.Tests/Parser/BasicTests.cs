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

namespace IronVelocity.Tests.Parser
{
    public class ParserTest
    {
        [TestCase("Sample Text")]
        [TestCase("$")]
        [TestCase("$$")]
        [TestCase("$!")]
        [TestCase("$!!")]
        [TestCase("$!!{")]
        [TestCase("$!$!!foo")]
        public void BasicTextTests(string input)
        {
            var result = ParseEnsuringNoErrors(input);

            var text = FlattenParseTree(result).OfType<VelocityParser.TextContext>().Single();
            Assert.That(text.GetText(), Is.EqualTo(input));
        }

        [TestCase("$foo")]
        [TestCase("$!bar")]
        [TestCase("${foo}")]
        [TestCase("$!{bar}")]
        public void BasicVariableReferences(string input)
        {
            var result = ParseEnsuringNoErrors(input);
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            Assert.That(flattened, Has.Exactly(1).InstanceOf<VelocityParser.ReferenceContext>());
        }

        [TestCase("$foo.dog", "dog")]
        [TestCase("$!bar.cat", "cat")]
        [TestCase("${foo.fish}", "fish")]
        [TestCase("$!{bar.bear}", "bear")]
        public void ReferenceWithPropertyInvocation(string input, string propertyName)
        {
            PrintTokens(input);
            var result = ParseEnsuringNoErrors(input);
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            var reference = flattened.OfType<VelocityParser.ReferenceContext>().Single();
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = flattened.OfType<VelocityParser.Property_invocationContext>().Single();
            Assert.That(property.GetText(), Is.EqualTo(propertyName));
        }


        [TestCase("$foo.dog()", "dog")]
        [TestCase("$!bar.cat()", "cat")]
        [TestCase("${foo.fish()}", "fish")]
        [TestCase("$!{bar.bear()}", "bear")]
        public void ReferenceWithZeroArgumentMethodInvocation(string input, string propertyName)
        {
            PrintTokens(input);
            var result = ParseEnsuringNoErrors(input);
            var flattened = FlattenParseTree(result);

            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());
            var reference = flattened.OfType<VelocityParser.ReferenceContext>().Single();
            Assert.That(reference.GetText(), Is.EqualTo(input));

            var property = flattened.OfType<VelocityParser.Method_invocationContext>().Single();
            Assert.That(property.GetText(), Is.EqualTo(propertyName + "()"));
        }

        [TestCase("Hello ", "$world")]
        [TestCase("$", "$foo")]
        [TestCase("$!", "$foo")]
        [TestCase("$!!{", "$foo")]
        public void TextFollowedByVariableReferences(string text, string reference)
        {
            var input = text + reference;
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);

            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();
            var referenceNode = flattened.OfType<VelocityParser.ReferenceContext>().Single();

            Assert.That(textNode.GetText(), Is.EqualTo(text));
            Assert.That(referenceNode.GetText(), Is.EqualTo(reference));
        }

        [TestCase("$color", " Box")]
        [TestCase("$salutation", ",")]
        [TestCase("$i", "$")]
        [TestCase("$bar", "$!")]
        [TestCase("$baz", "$!!")]
        [TestCase("$bat", "$!!{")]
        [TestCase("${formal}", "text")]
        [TestCase("$ref", ".")]
        [TestCase("${formal}", "}")]
        [TestCase("$informal", "}")]
        [TestCase("$informal", "}more")]
        [TestCase("$informal", "}}")]
        [TestCase("$variable", "(")]
        public void ReferenceFollowedByText(string reference, string text)
        {
            var input =  reference + text;
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);

            var referenceNode = flattened.OfType<VelocityParser.ReferenceContext>().Single();
            var textNode = flattened.OfType<VelocityParser.TextContext>().Single();

            Assert.That(referenceNode.GetText(), Is.EqualTo(reference));
            Assert.That(textNode.GetText(), Is.EqualTo(text));
        }

        [TestCase("$first", "$second")]
        [TestCase("${formal}", "$informal")]
        [TestCase("$informal", "${formal}")]
        [TestCase("${prefix}", "${suffix}")]
        public void TwoVariables(string reference1, string reference2)
        {
            var input = reference1 + reference2;
            var result = ParseEnsuringNoErrors(input);

            var flattened = FlattenParseTree(result);
            Assert.That(flattened, Has.No.InstanceOf<VelocityParser.TextContext>());

            var references = flattened.OfType<VelocityParser.ReferenceContext>()
                .Select(x => x.GetText())
                .ToList();

            Assert.That(references, Contains.Item(reference1));
            Assert.That(references, Contains.Item(reference2));
        }

        //If we start a formal reference, but don't' finish it, can't fallback to text
        [TestCase("${")]
        [TestCase("$${")]
        [TestCase("${foo")]
        [TestCase("${foo    ")]
        //TODO: investigate behavior around when un-closed parenthesis is allowed at the end of a reference
        // In NVelocity, the following are treated as text: "$foo.(", "$foo.(   ", "$foo.(."
        // But the following are errors "$foo.(123", "$foo.(123"
        [TestCase("$foo.bar( ")]
        [TestCase("$foo.bar(", Ignore = true, IgnoreReason = "Determine whether this should or shouldn't be a parse error")]
        [TestCase("$foo.bar(.", Ignore = true, IgnoreReason = "Determine whether this should or shouldn't be a parse error")]
        [TestCase("$foo.bar(hello", Ignore = true, IgnoreReason = "Determine whether this should or shouldn't be a parse error")]
        public void ShouldProduceParserError(string input)
        {
            var parser = CreateParser(input);

            var parsed = parser.template();

            if (parser.NumberOfSyntaxErrors == 0)
            {
                Console.WriteLine(parsed.ToStringTree(parser.TokenNames));
                Assert.Fail($"No Parse Errors Occurred;");
            }
        }






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
            }

        }

        protected IParseTree ParseEnsuringNoErrors(string input)
        {
            var parser = CreateParser(input);

            var parsed = parser.template();

            if (parser.NumberOfSyntaxErrors > 0)
            {
                Console.WriteLine(parsed.ToStringTree(parser.TokenNames));
                Assert.Fail($"{parser.NumberOfSyntaxErrors} errors occurred;");
            }
#if DEBUG
            else
            {
                Console.WriteLine(parsed.ToStringTree(parser.TokenNames));
            }
#endif

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
