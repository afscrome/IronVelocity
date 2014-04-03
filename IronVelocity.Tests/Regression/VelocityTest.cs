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
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using Tests;

    /// <summary>
    /// Test Velocity processing
    /// </summary>
    [TestFixture]
    public class VelocityTest
    {
        [Test]
        [TestCase("1 + 1", "2")]
        [TestCase("$fval + $fval", "2.4")]
        [TestCase("$dval + $dval", "10.6")]
        [TestCase("1 + $dval", "6.3")]
        [TestCase("1 + $fval", "2.2")]
        [TestCase("$fval + $dval", "6.50000004768372")]
        [TestCase("$fval * $dval", "6.36000025272369")]
        [TestCase("$fval - $dval", "-4.09999995231628")]
        [TestCase("$fval % $dval", "1.20000004768372")]
        [TestCase("$fval / $dval", "0.22641510333655")]
        public void MathOperations(string expression, string expectedResult)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var context = new Dictionary<string, object>();
            context["fval"] = 1.2f;
            context["dval"] = 5.3;

            var input = "#set($total = " + expression + ")\r\n$total";
            Utility.TestExpectedMarkupGenerated(input, expectedResult, context);
        }

        [TestCase("$firstName is my first name, my last name is $lastName", "Cort is my first name, my last name is Schaefer",TestName = "Evaluate_SimpleContext")]
        [TestCase("Hashtable lookup: foo=$hashtable.foo", "Hashtable lookup: foo=bar", TestName = "Evaluate_Nested1")]
        [TestCase("These are the nested properties:\naddr1=$contact.Address.Address1\naddr2=$contact.Address.Address2", "These are the nested properties:\naddr1=9339 Grand Teton Drive\naddr2=Office in the back",TestName = "Evaluate_Nested2")]
        [TestCase("Hashtable lookup: foo=$hashtable.foo", "Hashtable lookup: foo=bar", TestName = "Evaluate_Hashtable")]
        [TestCase("$!NOT_IN_CONTEXT", "", TestName="Evaluate_NotInContext")]
        [TestCase("#if($enumValue == \"Value2\")equal#end", "equal", TestName = "Evaluate_Enum1")]
        [TestCase("#if($enumValue == $EnumData.Value2)equal#end", "equal", TestName = "Evaluate_Enum2")]
        public void Test_Evaluate(string input, string expected)
        {
            var context = new Dictionary<string, object>();
            context["key"] = "value";
            context["firstName"] = "Cort";
            context["lastName"] = "Schaefer";

            Hashtable h = new Hashtable();
            h.Add("foo", "bar");
            context["hashtable"] = h;
            context["EnumData"] = typeof(EnumData);
            context["enumValue"] = EnumData.Value2;

            AddressData address = new AddressData();
            address.Address1 = "9339 Grand Teton Drive";
            address.Address2 = "Office in the back";
            context["address"] = address;

            ContactData contact = new ContactData();
            contact.Name = "Cort";
            contact.Address = address;
            context["contact"] = contact;

            Utility.TestExpectedMarkupGenerated(input, expected, context);
        }

        // inner classes to support tests --------------------------

        public class ContactData
        {
            private String name = String.Empty;
            private AddressData address = new AddressData();

            public String Name
            {
                get { return name; }
                set { name = value; }
            }

            public AddressData Address
            {
                get { return address; }
                set { address = value; }
            }
        }

        public class AddressData
        {
            private String address1 = String.Empty;
            private String address2 = String.Empty;

            public String Address1
            {
                get { return address1; }
                set { address1 = value; }
            }

            public String Address2
            {
                get { return address2; }
                set { address2 = value; }
            }
        }

        public enum EnumData
        {
            Value1,
            Value2,
            Value3
        }
    }
}