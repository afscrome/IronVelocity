using IronVelocity.Compilation.AST;
using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Tests
{
    [TestFixture]
    public class SymbolInformationTests
    {
        private IParser _parser = new AntlrVelocityParser();

        [TestCase("4 + 47", TestName="ParsingAddExpression_DetectsCorrectLineInfo")]
        [TestCase("73 - 21", TestName="ParsingSubtractExpression_DetectsCorrectLineInfo")]
        [TestCase("4 * 4", TestName = "ParsingMultiplyExpression_DetectsCorrectLineInfo")]
        [TestCase("87 / 3", TestName = "ParsingDivideExpression_DetectsCorrectLineInfo")]
        [TestCase("25 % 3", TestName = "ParsingModuloExpression_DetectsCorrectLineInfo")]
        public void MathematicalExpression(string expression)
        {
            var input = String.Format("#set($x = {0})", expression);
            var expectedSymbol = new SourceInfo(1, 11, 1, 11 + expression.Length - 1);

            var expressionTree = _parser.Parse(input, "test");
            var node = expressionTree.Flatten().OfType<MathematicalExpression>().Single();

            Assert.That(node.SourceInfo, Is.EqualTo(expectedSymbol));
        }


        [TestCase("$cash > 827", TestName = "ParsingGreaterThanExpression_DetectsCorrectLineInfo")]
        [TestCase("$dosh >= 823", TestName = "ParsingGreaterThanOrEqualExpression_DetectsCorrectLineInfo")]
        [TestCase("$dough < 038", TestName = "ParsingLessThanExpression_DetectsCorrectLineInfo")]
        [TestCase("$coin >= 23", TestName = "ParsingLessThanOrEqualExpression_DetectsCorrectLineInfo")]
        [TestCase("$note == 'test'", TestName = "ParsingEqualityExpression_DetectsCorrectLineInfo")]
        [TestCase("$card != 123", TestName = "ParsingInequalityExpression_DetectsCorrectLineInfo")]
        public void ComparisonExpression(string expression)
        {
            var input = String.Format("#if({0})Boo#end", expression);
            var expectedSymbol = new SourceInfo(1, 5, 1, 5 + expression.Length - 1);

            var expressionTree = _parser.Parse(input, "test");
            var node = expressionTree.Flatten().OfType<ComparisonExpression>().Single();

            Assert.That(node.SourceInfo, Is.EqualTo(expectedSymbol));
        }


        [Test]
        public void ParsingDottedExpression_DetectsCorrectLineInfo()
        {
            var input = "#set($x = $user.Balance.Add($deposit))";
            var expressionTree = _parser.Parse(input, "test");

            var setDirective = expressionTree.Flatten().OfType<SetDirective>().Single();
            var innerExpression = setDirective.Right;

            //var reference = innerExpression.Flatten().OfType<ReferenceExpression>().Single();
            var property = innerExpression.Flatten().OfType<PropertyAccessExpression>().Single();
            var method = innerExpression.Flatten().OfType<MethodInvocationExpression>().Single();

            //Assert.That(reference.Symbols, Is.EqualTo(new SymbolInformation(1, 11, 1, 37)));
            Assert.That(property.SourceInfo, Is.EqualTo(new SourceInfo(1, 17, 1, 23)));
            Assert.That(method.SourceInfo, Is.EqualTo(new SourceInfo(1, 25, 1, 37)));
        }

    }


    public static class ExpresionTreeFlattener
    {
        public static IEnumerable<Expression> Flatten(this Expression expression)
        {
            var visitor = new ExpressionFlattenerVisitor();
            visitor.Visit(expression);
            return visitor.Expressions;
        }


        private class ExpressionFlattenerVisitor : ExpressionVisitor
        {
            public readonly ICollection<Expression> Expressions = new List<Expression>();

            public override Expression Visit(Expression node)
            {
                Expressions.Add(node);
                return base.Visit(node);
            }
        }
    }

    
    

}
