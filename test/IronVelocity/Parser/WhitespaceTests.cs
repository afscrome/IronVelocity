using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
	public class WhitespaceTests : ParserTestBase
	{
		[TestCase(" ")]
		[TestCase("\t")]
		[TestCase("\t \t \t")]
		[TestCase("\n")]
		[TestCase("\r\n")]
		[TestCase("  \n \r\n \t")]
		public void ParseWhitespace(string input)
		{
			var literal = Parse(input, x => x.whitespace());
			Assert.That(literal.GetText(), Is.EqualTo(input));
		}
	}
}
