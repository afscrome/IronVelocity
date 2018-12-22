using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.TemplateExecution
{
	[TestFixture(StaticTypingMode.AsProvided)]
	[TestFixture(StaticTypingMode.PromoteContextToGlobals)]
	public class WhitespaceReductionTests : TemplateExeuctionBase
	{
		public WhitespaceReductionTests(StaticTypingMode mode) : base(mode)
		{
		}

		[TestCase(" ")]
		[TestCase("\t")]
		[TestCase("          ")]
		[TestCase("\t\t\t\t")]
		[TestCase("  \t  \t   ")]
		public void WhenRenderingHorizontalWhitespace_ASingleSpaceIsEmitted(string input)
		{
			var result = ExecuteTemplate(input, reduceWhitespace: true);
			Assert.That(result.Output, Is.EqualTo(" "));
		}

		[TestCase("\n")]
		[TestCase("\r\n")]
		[TestCase("\r\n\r\n\n\n")]
		[TestCase(" \r\n \t \n")]
		public void WhenRenderingVerticalWhitespace_ASingeNewlineIsEmitted(string input)
		{
			var result = ExecuteTemplate(input, reduceWhitespace: true);
			Assert.That(result.Output, Is.EqualTo("\n"));

		}

	}
}
