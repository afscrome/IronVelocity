using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var result = Parse(input, x => x.if_block());

            Assert.That(result, Is.Not.Null);
        }
    }
}
