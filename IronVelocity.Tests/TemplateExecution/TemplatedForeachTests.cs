using NUnit.Framework;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(GlobalMode.AsProvided)]
    [TestFixture(GlobalMode.Force)]
    public class TemplatedForeachTests : TemplateExeuctionBase
    {
        public TemplatedForeachTests(GlobalMode mode) : base(mode)
        {
        }

        [Test]
        public void ShouldRenderNoDataSectionWhenEnumerableHasNoItems()
        {
            var input = "#foreach($x in [])ABC#nodata Correct#end";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(" Correct"));
        }

        [Test]
        public void ShouldRenderNoDataSectionForInvalidEnumerable()
        {
            var input = "#foreach($x in 3.142)ABC#nodata Success#end";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(" Success"));

        }

        [Test]
        public void ShouldNotRenderNoDataSectionWhenEnumerableHasItems()
        {
            var input = "#foreach($x in [1,2])ABC#nodata Success#end";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo("ABCABC"));

        }

        [Test]
        public void ShouldRenderEachSectionWhenEnumerableHasItems()
        {
            var input = "#foreach($x in [1..2])#each$x,#end";
            var expected = "1,2,";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldRenderBeforeAlllWhenEnumerableHasItems()
        {
            var input = @"#foreach($x in [1,2,3])#beforeall BEFORE#each $x#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(" BEFORE 1 2 3"));
        }

        [Test]
        public void ShouldRenderAfterAllWhenEnumerableHasItems()
        {
            var input = @"#foreach($x in [1,2,3])#each $x#afterall AFTER#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(" 1 2 3 AFTER"));
        }

        [Test]
        public void ShouldRenderBeforeWhenEnumerableHasItems()
        {
            var input = @"#foreach($x in [1,2,3])#before BEFORE#each $x#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(" BEFORE 1 BEFORE 2 BEFORE 3"));
        }

        [Test]
        public void ShouldRenderAfterWhenEnumerableHasItems()
        {
            var input = @"#foreach($x in [1,2,3]) $x#after AFTER#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(" 1 AFTER 2 AFTER 3 AFTER"));
        }

        [Test]
        public void ShouldRenderBetweenWhenEnumerableHasItems()
        {
            var input = @"#foreach($x in [5,6,7,8])$x#between,#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo("5,6,7,8"));
        }

        [Test]
        public void ShouldRenderOddWhenEnumerableHasItems()
        {
            var input = @"#foreach($x in [1,2,3,4,5])$x #odd!#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo("!1 2 !3 4 !5 "));
        }

        [Test]
        public void ShouldRenderEvenWhenEnumerableHasItems()
        {
            var input = @"#foreach($x in [1,2,3,4,5])$x #even+#end";
            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo("1 +2 3 +4 5 "));
        }

        [Test]
        public void ShouldRenderFullTemplateWhenEnumerableHasItems()
        {
            var input = @"#foreach($item in ['A','B','C'])#beforeall <<#before [#each $item#after ]#between ,#odd +#even -#afterall >>#nodata Nada#end";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(" << [ + A ] , [ - B ] , [ + C ] >>"));

        }

        [Test]
        public void ShouldOnlyRenderNoDataWhenEnumerableIsInvalid()
        {
            var input = @"#foreach($item in $fake)#beforeall <<#before [#each $item#after ]#between ,#odd +#even -#afterall >>#nodata Nada#end";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(" Nada"));

        }

        [Test]
        public void ShouldOnlyRenderNoDataWhenEnumerableIsEmpty()
        {
            var input = @"#foreach($item in [])#beforeall <<#before [#each $item#after ]#between ,#odd +#even -#afterall >>#nodata Nada#end";

            var result = ExecuteTemplate(input);

            Assert.That(result.Output, Is.EqualTo(" Nada"));

        }
    }
}
