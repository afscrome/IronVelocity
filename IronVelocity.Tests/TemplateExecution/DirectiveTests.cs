using NUnit.Framework;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronVelocity.Directives;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(GlobalMode.AsProvided)]
    [TestFixture(GlobalMode.Force)]
    public class DirectiveTests : TemplateExeuctionBase
    {
        public DirectiveTests(GlobalMode mode) : base(mode)
        {
        }

        [TestCase("#custom", "This directive has 0 arguments")]
        [TestCase("#custom()", "This directive has 0 arguments")]
        [TestCase("#custom   ()", "This directive has 0 arguments")]
        [TestCase("#custom(   )", "This directive has 0 arguments")]
        [TestCase("#custom( 123  )", "This directive has 1 arguments")]
        [TestCase("#custom( 123 'hello world'  )", "This directive has 2 arguments")]
        public void SingleLineDirectiveTest(string input, string expected)
        {
            var directives = new[] { new SingleLineDirective() };

            var result = ExecuteTemplate(input, customDirectives: directives);

            Assert.That(result.Output, Is.EqualTo(expected));

        }

        [TestCase("#multiLine()BODY#end", "This block directive has 0 arguments")]
        public void BlockDirectiveTest(string input, string expected)
        {
            var directives = new[] { new BlockDirective() };

            var result = ExecuteTemplate(input, customDirectives: directives);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [TestCase("#-")]
        [TestCase("#unknown")]
        public void ShouldPrintUnrecognisedDirectiveAsIs(string input)
        {
            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(input));
        }




        private class SingleLineDirective : CustomDirectiveBuilder
        {
            public override string Name => "custom";
            public override bool IsBlockDirective => false;

            public override Expression Build(IReadOnlyList<Expression> arguments, Expression body)
                => Expression.Constant($"This directive has {arguments.Count} arguments");
        }

        private class BlockDirective : CustomDirectiveBuilder
        {
            public override string Name => "multiLine";
            public override bool IsBlockDirective => true;

            public override Expression Build(IReadOnlyList<Expression> arguments, Expression body)
            {
                Assert.That(body, Is.Not.Null);
                return Expression.Constant($"This block directive has {arguments.Count} arguments");
            }
        }

    }
}
