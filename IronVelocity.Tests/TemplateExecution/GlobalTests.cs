using NUnit.Framework;
using System;

namespace IronVelocity.Tests.TemplateExecution
{
    public class GlobalTests : TemplateExeuctionBase
    {
        public GlobalTests() : base(GlobalMode.Force) { }

        [Test]
        public void ShouldRejectNullGlobalVariable()
        {
            var globals = new { foo = (object)null };

            Assert.That(() => ExecuteTemplate("", globals: globals), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
        
    }
}
