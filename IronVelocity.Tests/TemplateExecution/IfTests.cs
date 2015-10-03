using NUnit.Framework;
using System;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture]
    public class IfTests : TemplateExeuctionBase
    {
        [Test]
        public void IfTrue()
        {
            var input = "#if(true)foo#end";
            var expected = "foo";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }
        [Test]
        public void IfFalse()
        {
            var input = "#if(false)foo#end";
            var expected = "";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void IfElseTrue()
        {
            var input = "#if(true)foo#else bar#end";
            var expected = "foo";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void IfElseFalse()
        {
            var input = "#if(false)foo#else bar#end";
            var expected = " bar";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void IfElseIfElseFirstMatch()
        {
            var input = "#if(true)foo#elseif(true)bar#else baz#end";
            var expected = "foo";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void IfElseIfSecondMatch()
        {
            var input = "#if(false)foo#elseif(true)bar#else baz#end";
            var expected = "bar";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void IfElseIfFinalMatch()
        {
            var input = "#if(false)foo#elseif(false)bar#else baz#end";
            var expected = " baz";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }
        [Test]
        public void IfElseIfNoMatch()
        {
            var input = "#if(false)foo#elseif(false)bar#end";
            var expected = "";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }


        [Test]
        public void IfWithCoercionTrue()
        {
            var input = "#if('hello')foo#end";
            var expected = "foo";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void IfWithCoercionFalse()
        {
            var input = "#if($null)foo#end";
            var expected = "";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void When_MultipleElseIfMatches_Should_ExecuteFirstMatching()
        {
            var input = "#if(false)ONE#elseif(true)TWO#elseif(true)THREE#end";
            var expected = "TWO";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldRenderBodyOfIfStatementWithTrueVariable()
        {
            var input = "#if($Value)Has Content#end";
            var context = new { Value = true };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Output, Is.EqualTo("Has Content"));
        }

        [Test]
        public void ShouldRenderBodyOfIfStatementWithValueTypeVariable()
        {
            var input = "#if($Value)Has Content#end";
            var context = new { Value = Guid.NewGuid() };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Output, Is.EqualTo("Has Content"));
        }

        [Test]
        public void ShouldRenderBodyOfIfStatementWithNotNullReferenceTypeVariable()
        {
            var input = "#if($Value)Has Content#end";
            var context = new { Value = new object() };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Output, Is.EqualTo("Has Content"));
        }

        [Test]
        public void ShouldNotRenderBodyOfIfStatementWithFalseVariable()
        {
            var input = "#if($Value)Has Content#end";
            var context = new { Value = false };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Output, Is.Empty);
        }

        [Test]
        public void ShouldNotRenderBodyOfIfStatementWithNullReferenceTypeVariable()
        {
            var input = "#if($Value)Has Content#end";
            var context = new { Value = (object)null };

            var execution = ExecuteTemplate(input, locals: context);

            Assert.That(execution.Output, Is.Empty);
        }
    }

  
}
