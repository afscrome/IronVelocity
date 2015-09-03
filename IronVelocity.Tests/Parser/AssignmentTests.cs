using IronVelocity.Parser;
using NUnit.Framework;
using System;

namespace IronVelocity.Tests.Parser
{
    public class AssignmentTests : ParserTestBase
    {
        [TestCase("$x=123", "$x", "123")]
        [TestCase("$x.prop=123", "$x.prop", "123")]
        [TestCase("$x.method().prop=123", "$x.method().prop", "123")]
        [TestCase("$x = 123", "$x", "123")]
        [TestCase(" $x = 123 ", "$x", "123")]
        public void ParseAssignmentExpression(string input, string left, string right)
        {
            //Can't use base.ParseBinaryExpressionTest(..) here due to the way in which Set deals with internal whitespace
            //TODO: can this be fixed in the grammar?
            var result = CreateParser(input, VelocityLexer.ARGUMENTS).assignment();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetText(), Is.EqualTo(input));

            Assert.That(result.reference().GetText().Trim(), Is.EqualTo(left));
            Assert.That(result.argument().GetText().Trim(), Is.EqualTo(right));
        }
    }
}
