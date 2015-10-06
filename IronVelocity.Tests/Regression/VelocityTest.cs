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
// This file has been modified from the original to allow the tests to run
// against templates produced by IronVelocity, as well as to split multiple
// methods into separate test cases

namespace NVelocity.Test
{
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using IronVelocity.Tests;

    /// <summary>
    /// Test Velocity processing
    /// </summary>
    [TestFixture]
    public class VelocityTest
    {
        [TestCase("#if($enumValue == $EnumData.Value2)equal#end", "equal", TestName = "Evaluate_Enum2")]
        public void Test_Evaluate(string input, string expected)
        {
            var context = new Dictionary<string, object>();

            Hashtable h = new Hashtable();
            h.Add("foo", "bar");
            context["EnumData"] = typeof(EnumData);
            context["enumValue"] = EnumData.Value2;

            Utility.TestExpectedMarkupGenerated(input, expected, context);
        }

        public enum EnumData
        {
            Value1,
            Value2,
            Value3
        }
    }
}