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
            var parser = new VelocityParser(tokenStream)
            {
                ErrorHandler = new TestBailErrorStrategy(input)
            };

            return parser;
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
