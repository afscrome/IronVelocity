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
    using System.Collections.Generic;
    using IronVelocity.Tests;

	/// <summary>
	/// Tests comparison of custom objects.
	/// </summary>
	[TestFixture]
	public class ObjectComparisonTestCase
	{
		#region Constants

		private const string VmTemplate =
			@"#if ($left == $right )equal
#else
different
#end
#if ($left > $right)
greater
#end
#if ($left < $right)
smaller
#end
#if ($left >= $right)
geq
#end
#if ($left <= $right)
leq
#end";

		private const string CmpEqual = @"equal
geq
leq
";
		private const string CmpGreater = @"different
greater
geq
";
		private const string CmpSmaller = @"different
smaller
leq
";

		#endregion

		private int Int = 11;
		private ulong ULong = 12UL;
		private float Float = 13.878F;
		private double Double = 15.059D;

		private IDictionary<string,object> ctx = new Dictionary<string,object>();



		[Test]
		public void Equivalence()
		{
			String eqTest = "#set($int = 1)\r\n" +
			                "#set($str = \"str\")\r\n" +
			                "#set($bool = true)\r\n" +
			                "#if( $int == $str)\r\n" +
			                "wrong\r\n" +
			                "#else\r\n" +
			                "right\r\n" +
			                "#end\r\n" +
			                "#if( $int == 1 )\r\n" +
			                "right\r\n" +
			                "#else\r\n" +
			                "wrong\r\n" +
			                "#end\r\n" +
			                "#if ( $int == 2 )\r\n" +
			                "wrong\r\n" +
			                "#else\r\n" +
			                "right\r\n" +
			                "#end\r\n" +
			                "#if( $str == 2 )\r\n" +
			                "wrong\r\n" +
			                "#else\r\n" +
			                "right\r\n" +
			                "#end\r\n" +
			                "#if( $str == \"str\")\r\n" +
			                "right\r\n" +
			                "#else\r\n" +
			                "wrong\r\n" +
			                "#end\r\n" +
			                "#if( $str == $nonexistantreference )\r\n" +
			                "wrong\r\n" +
			                "#else\r\n" +
			                "right\r\n" +
			                "#end\r\n" +
			                "#if( $str == $bool )\r\n" +
			                "wrong\r\n" +
			                "#else\r\n" +
			                "right\r\n" +
			                "#end\r\n" +
			                "#if ($bool == true )\r\n" +
			                "right\r\n" +
			                "#else\r\n" +
			                "wrong\r\n" +
			                "#end\r\n" +
			                "#if( $bool == false )\r\n" +
			                "wrong\r\n" +
			                "#else\r\n" +
			                "right\r\n" +
			                "#end\r\n";

            Utility.TestExpectedMarkupGenerated(eqTest,"right\r\nright\r\nright\r\nright\r\nright\r\nright\r\nright\r\nright\r\nright\r\n");
            Utility.TestExpectedMarkupGenerated(eqTest,"right\r\nright\r\nright\r\nright\r\nright\r\nright\r\nright\r\nright\r\nright\r\n");

		}
        /*
		[Test]
		public void CompareEnums()
		{
			Assert.AreEqual(ObjectComparer.Equal, ObjectComparer.CompareObjects(FileAccess.Read, FileAccess.Read));
			Assert.AreEqual(ObjectComparer.Equal, ObjectComparer.CompareObjects(FileAccess.Read, "Read"));
			Assert.AreEqual(ObjectComparer.Equal, ObjectComparer.CompareObjects("Read", FileAccess.Read));
			Assert.AreEqual(ObjectComparer.Greater, ObjectComparer.CompareObjects("Reader", FileAccess.Read));
		}
        */

		[Test]
		public void ComparePrimitive()
		{
            /*
			Assert.AreEqual(ObjectComparer.Equal, ObjectComparer.CompareObjects(Int, Int));
			Assert.AreEqual(ObjectComparer.Smaller, ObjectComparer.CompareObjects(Int, Double));
			Assert.AreEqual(ObjectComparer.Greater, ObjectComparer.CompareObjects(Double, Int));
			Assert.AreEqual(ObjectComparer.Smaller, ObjectComparer.CompareObjects(Float, Double));
			Assert.AreEqual(ObjectComparer.Greater, ObjectComparer.CompareObjects(Double, Float));
			Assert.AreEqual(ObjectComparer.Smaller, ObjectComparer.CompareObjects(Int, Float));
			Assert.AreEqual(ObjectComparer.Greater, ObjectComparer.CompareObjects(Float, Int));
			Assert.AreEqual(ObjectComparer.Smaller, ObjectComparer.CompareObjects(ULong, Float));
			Assert.AreEqual(ObjectComparer.Greater, ObjectComparer.CompareObjects(Double, ULong));
			Assert.AreEqual(ObjectComparer.Smaller, ObjectComparer.CompareObjects(Int, ULong));
            */
			VmCompareCouple(Int, Double);
			VmCompareCouple(Int, Float);
			VmCompareCouple(Int, ULong);
			VmCompareCouple(ULong, Float);
			VmCompareCouple(Float, Double);
			VmCompareCouple(ULong, Double);
            VmCompareCouple(Int, ULong);
		}

		/// <summary>
		/// String does an alphabetical comparision, if you compare an object to a string, the
		/// string value of that object will be used.  Char does an ascii comparison.
		/// </summary>
		[Test]
        [Ignore("Currently not supporting non equality comparisons for strings")]
		public void CompareString()
		{
			string aaa = "aaa";
			string bbb = "aab";

			VmCompareCouple(aaa, bbb);

			char c = 'c';
			short s = 7;

			VmCompareCouple(aaa, c);
			VmCompareCouple(bbb, c);

			VmCompareCouple(s, aaa);
			VmCompareCouple(s, c);
		}

		[Test]
		public void CompareDateTime()
		{
			DateTime now = DateTime.Now;
			DateTime next = now.AddSeconds(3);

			VmCompareCouple(now, next);
		}

		[Test]
		public void CompareTimeSpan()
		{
			TimeSpan longer = new TimeSpan(15, 12, 37, 12);
			TimeSpan shorter = new TimeSpan(17, 56, 59);

            VmCompareCouple(shorter, longer);
		}

		public void VmCompareCouple(object small, object big)
		{
			ctx["left"] =small;
			ctx["right"] =small;

            Utility.TestExpectedMarkupGenerated(VmTemplate, CmpEqual,ctx);

			ctx["left"] =big;
			ctx["right"] =big;

            Utility.TestExpectedMarkupGenerated(VmTemplate, CmpEqual, ctx);

			ctx["left"] =small;
			ctx["right"] =big;

            Utility.TestExpectedMarkupGenerated(VmTemplate, CmpSmaller, ctx);

			ctx["left"] =big;
			ctx["right"] =small;

            Utility.TestExpectedMarkupGenerated(VmTemplate, CmpGreater, ctx);
		}
	}
}