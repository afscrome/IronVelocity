using NUnit.Framework;


namespace IronVelocity.Tests.TemplateExecution
{
    //TODO: more tests needed here for the various template sections (BeforeAll, Before etc.)
    //But holding off on those until the directive whitespace gobbling has been fixed.
    public class TemplatedForeachTests : TemplateExeuctionBase
    {
        [Test]
        public void ShouldRenderNoDataSectionForNullEnumerable()
        {
            var input = "#foreach($x in $null)ABC#nodata Correct#end";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(" Correct"));
        }

        [Test]
        public void ShouldRenderNoDataSectionForBadEnumerable()
        {
            var input = "#foreach($x in 3.142)ABC#nodata Success#end";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(" Success"));

        }

        [Test]
        public void ShouldRenderEachSection()
        {
            var input = "#foreach($x in [1..2])#each$x,#end";
            var expected = "1,2,";

            var result = ExecuteTemplate(input);
            Assert.That(result.Output, Is.EqualTo(expected));
        }

    }
}
