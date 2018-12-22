using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class EscapeTests : TemplateExeuctionBase
    {
        public EscapeTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase(@"\#set")]
        [TestCase(@"\#if")]
        [TestCase(@"\#elseif")]
        [TestCase(@"\#else")]
        [TestCase(@"\#end")]
        [TestCase(@"\#set(")]
        [TestCase(@"\#if(")]
        [TestCase(@"\#elseif(")]
        [TestCase(@"\#else(")]
        [TestCase(@"\#end(")]
        public void ShouldRenderEscapedDefinedDirectiveAsInSource(string input)
        {
            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(input.Substring(1)));
        }

        [TestCase(@"\$x")]
        [TestCase(@"\${x}")]
        [TestCase(@"\$!x")]
        [TestCase(@"\$!{x.silenced}")]
        [TestCase(@"\$x.bat")]
        [TestCase(@"\$x.trooper(123)")]
        [TestCase(@"\$x.buzz( $fizzbuzz )")]
        [TestCase(@"\$!{x.formal.silenced}")]
        public void ShouldRenderDefinedVariableAsInSource(string input)
        {
            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(input.Substring(1)));
        }
    }
}
