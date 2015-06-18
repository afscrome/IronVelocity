using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.ParserTests
{
    public class ListExpressionTests
    {
        [TestCase("[]")]
        [TestCase("[    ]")]
        public void EmptyList(string input)
        {
            var parser = new VelocityParser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<ListExpressionNode>());

            var node = (ListExpressionNode)result;

            Assert.That(node.Values.Count, Is.EqualTo(0));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

        [TestCase("[123]")]
        [TestCase("[  123]")]
        [TestCase("[123  ]")]
        [TestCase("[  123    ]")]
        public void ListWithSingleElement(string input)
        {
            var parser = new VelocityParser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<ListExpressionNode>());

            var node = (ListExpressionNode)result;

            Assert.That(node.Values.Count, Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

        [TestCase("['test',4.5,$variable,true]")]
        [TestCase("[   'test',   4.5,   $variable,   true]")]
        [TestCase("[   'test'   ,   4.5   ,   $variable   ,   true   ]")]
        [TestCase("['test'    ,4.5    ,$variable    ,true    ]")]
        public void ListWithMultipleElements(string input)
        {
            var parser = new VelocityParser(input);
            var result = parser.Expression();

            Assert.That(result, Is.TypeOf<ListExpressionNode>());

            var node = (ListExpressionNode)result;

            Assert.That(node.Values.Count, Is.EqualTo(4));

            Assert.That(node.Values[0], Is.TypeOf<StringNode>());
            var firstArg = (StringNode)node.Values[0];
            Assert.That(firstArg.Value, Is.EqualTo("test"));

            Assert.That(node.Values[1], Is.TypeOf<FloatingPointNode>());
            var secondArg = (FloatingPointNode)node.Values[1];
            Assert.That(secondArg.Value, Is.EqualTo(4.5));

            Assert.That(node.Values[2], Is.TypeOf<ReferenceNode>());
            var thirdArg = (ReferenceNode)node.Values[2];
            //Assert.That(thirdArg..Value, Is.EqualTo("test"));

            Assert.That(node.Values[3], Is.EqualTo(BooleanNode.True));
            Assert.That(parser.HasReachedEndOfFile, Is.True);
        }

    }
}
