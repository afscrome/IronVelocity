using Antlr4.Runtime;
using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;

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
        [TestCase("${formal}", "}")]
        [TestCase("$informal", "}")]
        [TestCase("$informal", "}more")]
        [TestCase("$informal", "}}")]
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



        //TODO: investigate behaviour around when un-closed paranthesis is allowed at the end of a reference
        // In NVelocity, the followign are treated as text: "$foo.(", "$foo.(   ", "$foo.(."
        // But the followign are errors "$foo.(123", "$foo.(123"


        protected VelocityParser CreateParser(string input)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new VelocityLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            return new VelocityParser(tokenStream);
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

    }
}
