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
    public class ParenthesisedExpressionTests
    {
        [Test]
        public void ParenthesisedExpression()
        {
            var parser = new VelocityParserWithStatistics("(482)", LexerState.Vtl);
            var result = parser.Expression();

            //Assert.That(parser., Is.EqualTo(1));
            Assert.That(parser.HasReachedEndOfFile);

            Assert.That(result, Is.TypeOf<UnaryExpressionNode>());
            var node = (UnaryExpressionNode)result;

            Assert.That(node.Operation, Is.EqualTo(UnaryOperation.Parenthesised));
            Assert.That(node.Value, Is.TypeOf<IntegerLiteralNode>());
        }
    }
}
