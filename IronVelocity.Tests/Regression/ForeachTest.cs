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
    using IronVelocity.Tests.Regression.Provider;
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using IronVelocity.Tests;

	/// <summary>
	/// Tests to make sure that the VelocityContext is functioning correctly
	/// </summary>
	[TestFixture]
	public class ForeachTest
	{
		private ArrayList items;
		private IDictionary<string,object> c = new Dictionary<string,object>();
		private string template;

		[SetUp]
		public void Setup()
		{
			items = new ArrayList();

			items.Add("a");
			items.Add("b");
			items.Add("c");
			items.Add("d");

			c["x"] = new Something();
			c["items"]=items;


			template =
				@"
						#foreach( $item in $items )
						#before
							(
						#each
							$item
						#after
							)
						#between  
							,
						#odd  
							+
						#even  
							-
						#nodata
							nothing
						#beforeall
							<
						#afterall
							>
						#end
			";
		}

		[Test]
		public void SimpleForeach()
		{
            var output = Utility.GetNormalisedOutput(@"
					#foreach( $item in $items )
						$item,
					#end
				", c);

            Assert.AreEqual("a,b,c,d,", Normalize(output));
		}

		[Test]
		public void BeforeAfterForeach()
		{
			var input = @"
						#foreach( $item in $items )
							$item,
						#beforeall
							<
						#afterall
							>
						#end
					";

            var output = Utility.GetNormalisedOutput(input, c);
			Assert.AreEqual("<a,b,c,d,>", Normalize(output));
		}

		[Test]
		public void TemplateForeachAllSections()
		{
            var output = Utility.GetNormalisedOutput(template, c);
			Assert.AreEqual("<(+a),(-b),(+c),(-d)>", Normalize(output));
		}

		[Test]
		public void TemplateForeachNoDataSection()
		{
			items.Clear();
            var output = Utility.GetNormalisedOutput(template, c);
			Assert.AreEqual("nothing", Normalize(output));
		}

		[Test]
		public void TemplateForeachNoDataSection2()
		{
			c["items"] = null;

            var output = Utility.GetNormalisedOutput(template, c);
			Assert.AreEqual("nothing", Normalize(output));
		}

		[Test]
		public void TemplateForeachTwoItems()
		{
			items.Clear();
			items.Add("a");
			items.Add("b");

            var output = Utility.GetNormalisedOutput(template, c);

			Assert.AreEqual("<(+a),(-b)>", Normalize(output));
		}

		[Test]
		public void TemplateForeachOneItem()
		{
			items.Clear();
			items.Add("a");

            var output = Utility.GetNormalisedOutput(template, c);
			Assert.AreEqual("<(+a)>", Normalize(output));
		}

		[Test]
		public void ParamArraySupportAndForEach2()
		{
            var localContext = new Dictionary<string, object>();

            var items = new ArrayList();
			items.Add("a");
			items.Add("b");
			items.Add("c");

            localContext["x"] = new Something();
            localContext["items"] = items;

			var input =      "#foreach( $item in $items )\r\n" +
			                 "#if($item == \"a\")\r\n $x.Contents( \"x\", \"y\" )#end\r\n" +
			                 "#if($item == \"b\")\r\n $x.Contents( \"x\" )#end\r\n" +
			                 "#if($item == \"c\")\r\n $x.Contents( \"c\", \"d\", \"e\" )#end\r\n" +
			                 "#end\r\n";

            var output = Utility.GetNormalisedOutput(input, localContext);
			Assert.AreEqual(" x,y x c,d,e", output);
		}

		[Test]
		public void ForEachSimpleCase()
		{
            ArrayList items = new ArrayList();

			items.Add("a");
			items.Add("b");
			items.Add("c");

            var localContext = new Dictionary<string, object>();
            localContext["x"] = new Something2();
			localContext["items"] = items;
			localContext["d1"] = new DateTime(2005, 7, 16);
			localContext["d2"] = new DateTime(2005, 7, 17);
			localContext["d3"] = new DateTime(2005, 7, 18);


            var input =      "#foreach( $item in $items )\r\n" +
			                 "#if($item == \"a\")\r\n $x.FormatDate( $d1 )#end\r\n" +
			                 "#if($item == \"b\")\r\n $x.FormatDate( $d2 )#end\r\n" +
			                 "#if($item == \"c\")\r\n $x.FormatDate( $d3 )#end\r\n" +
			                 "#end\r\n";

            Utility.TestExpectedMarkupGenerated(input, " 16 17 18", localContext);
		}

		[Test]
		public void Hashtable1()
		{
            var localContext = new Dictionary<string, object>();

			Hashtable x = new Hashtable();
			x.Add("item", "value1");

            localContext["x"] = x;

            Utility.TestExpectedMarkupGenerated("$x.get_Item( \"item\" )", "value1", localContext);
		}


        private string Normalize(string s)
		{
			return Regex.Replace(s, "\\s+", string.Empty);
		}

	}
}