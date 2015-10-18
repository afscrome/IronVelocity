using NUnit.Framework;

namespace IronVelocity.Tests.Parser
{
    public class IfTests : ParserTestBase
    {

        [TestCase("#if(true)#end")]
        [TestCase("#if( true)#end")]
        [TestCase("#if(true )#end")]
        [TestCase("#if( true )#end")]
        [TestCase("#if( !(true) )#end")]
        public void ShouldParseBasicIf(string input)
        {
            var result = Parse(input, x => x.ifBlock());

            Assert.That(result, Is.Not.Null);
        }
    }
}
