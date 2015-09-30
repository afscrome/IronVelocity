using NUnit.Framework;

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
            var expected = "bar";

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
            var expected = "baz";

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
    }

  
}
