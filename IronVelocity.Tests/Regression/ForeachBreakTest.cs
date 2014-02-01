// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
namespace NVelocity.Test
{
    using NUnit.Framework;
    using System;
    using Tests;

    /// <summary>
    /// Tests to make sure that the VelocityContext is functioning correctly
    /// </summary>
    [TestFixture]
    public class ForeachBreakTest
    {
        /**
         * Tests break directive with a couple of iterations.
         */
        [Test]
        [Ignore("Break not yet implemented")]
        public void testConditionalBreakDirective()
        {
            assertEvalEquals("1, 2, 3, 4, 5",
                             @"#foreach($i in [1..10])
$i#if($i > 4)#break
#end, #end");
        }

        /**
         * Tests break directive with immediate break.
         */
        [Test]
        [Ignore("Break not yet implemented")]
        public void testUnconditionalBreakDirective()
        {
            assertEvalEquals("1", @"#foreach($i in [1..5])
$i#break
#end");
        }

        [Test]
        [Ignore("Break not yet implemented")]
        public void testNestedForeach()
        {
            assertEvalEquals("~~~, ~~, ~, ",
                @"#foreach($i in [1..3])
#foreach($j in [2..4])
#if($i*$j >= 8)#break
#end~#end, #end");
        }

        protected void assertEvalEquals(String expected, String template)
        {
            Utility.TestExpectedMarkupGenerated(template, expected);
        }
    }
}