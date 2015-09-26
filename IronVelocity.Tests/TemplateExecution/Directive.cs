using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace IronVelocity.Tests.TemplateExecution
{
    public class Directive : TemplateExeuctionBase
    {

        [TestCase("#custom", "This directive has 0 arguments")]
        [TestCase("#custom()", "This directive has 0 arguments")]
        [TestCase("#custom   ()", "This directive has 0 arguments")]
        [TestCase("#custom(   )", "This directive has 0 arguments")]
        [TestCase("#custom( 123  )", "This directive has 1 arguments")]
        [TestCase("#custom( 123 'hello world'  )", "This directive has 2 arguments")]
        public void SingleLineDirectiveTest(string input, string expected)
        {
            var directives = new[] { new TestDirective() };

            var result = ExecuteTemplate(input, customDirectives: directives);

            Assert.That(result.Output, Is.EqualTo(expected));

        }


        private class TestDirective : CustomDirective
        {
            public override string Name => "custom";
            public override bool AcceptsParameters => true;
            public override bool IsMultiline => false;

            public override Expression Build(IReadOnlyList<Expression> arguments, Expression body)
                => Expression.Constant($"This directive has {arguments.Count} arguments");
        }

    }
}
