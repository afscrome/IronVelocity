using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests;

namespace IronVelocity.Tests.Binders
{
    [TestFixture]
    public class ComparisonBinderTests
    {
        [TestCaseSource("TestCases")]
        public void Test(object left, object right, ComparisonOperation operation, bool expected)
        {
            ComparisonTest(left, right, operation, expected);
        }


        public IEnumerable<TestCaseData> TestCases()
        {
            var objectReference = new object();

            var guid1 = new Guid("c974771a-46a1-4c5c-9ba1-2e6b4c2eb5ba");
            var guid2 = new Guid("c974771a-46a1-4c5c-9ba1-2e6b4c2eb5ba");

            if (Object.ReferenceEquals(guid1, guid2))
                Assert.Inconclusive("guid1 and guid2 should not be reference equals");

            return new[] {
                GenerateTestCaseData(1, 1, true, false, false, "Equal Integers"),
                GenerateTestCaseData(3, 3F, true, false, false, "Equal Integer Float"),
                GenerateTestCaseData(7, 7L, true, false, false, "Equal Integer Long"),
                GenerateTestCaseData(1, TestEnum.Red, true, false, false, "Equal Integer Enum"),
                GenerateTestCaseData(TestEnum.Green, 2, true, false, false, "Equal Enum Integer"),
                GenerateTestCaseData(TestEnum.Blue, "Blue", true, false, false, "Equal Enum Identical Case String"),
                GenerateTestCaseData(TestEnum.Blue, "BLUE", false, false, false, "Equal Enum Differing Case String"),
                GenerateTestCaseData(TestEnum.Green, 1, false, true, false, "GreaterThan Enum"),
                GenerateTestCaseData(7, 7L, true, false, false, "NotEqual Enum String"),
                GenerateTestCaseData(true, true, true, false, false, "True True"),
                GenerateTestCaseData(true, false, false, false, false, "True False"),
                GenerateTestCaseData(false, true, false, false, false, "False True"),
                GenerateTestCaseData(false, false, true, false, false, "False False"),
                GenerateTestCaseData(null, null, true, false, false, "null null"),
                GenerateTestCaseData("foo", null, false, false, false, "object null"),
                GenerateTestCaseData(null, "bar", false, false, false, "null object"),
                GenerateTestCaseData("Hello World!", "Hello World!", true, false, false, "Identical Case Strings"),
                GenerateTestCaseData("HELLO WORLD!", "hello world!", false, false, false, "Differing Case Strings"),
                GenerateTestCaseData(DateTime.Now.AddMinutes(-1), DateTime.Now, false, false, true, "DateTime"),
                GenerateTestCaseData(objectReference, objectReference, true, false, false, "Object Identical References"),
                GenerateTestCaseData(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(50), false, true, false, "TimeSpan"),
                GenerateTestCaseData(guid1, guid2, true, false, false, "Structs with same values but different instances"),
                GenerateTestCaseData(new OverloadedEquals(), new OverloadedEquals(), true, false, false, "Object with Overloaded Equality Operators"),
                GenerateTestCaseData(new OverriddenEquals(), new OverriddenEquals(), true, false, false, "Object with Overriden Equals Method"),
            }.SelectMany(x => x);
        }


        private IEnumerable<TestCaseData> GenerateTestCaseData(object left, object right, bool areEqual, bool isGreaterThan, bool isLessThan, string testNamePrefix)
        {
            if (isGreaterThan && isLessThan)
                throw new InvalidOperationException("Two objects cannot be both greater & less than each other - " + testNamePrefix);
            if (areEqual && isGreaterThan)
                throw new InvalidOperationException("Two equal objects cannot be greater than each other - " + testNamePrefix);
            if (areEqual && isLessThan)
                throw new InvalidOperationException("Two equal objects cannot be less than each other - " + testNamePrefix);

            testNamePrefix = "ComparisonBinderTest - " + testNamePrefix;

            yield return new TestCaseData(left, right, ComparisonOperation.Equal, areEqual)
                .SetName(testNamePrefix + " - Equal");
            yield return new TestCaseData(left, right, ComparisonOperation.NotEqual, !areEqual)
                .SetName(testNamePrefix + " - NotEqual");
            yield return new TestCaseData(left, right, ComparisonOperation.GreaterThan, isGreaterThan)
                .SetName(testNamePrefix + " - GreaterThan");
            yield return new TestCaseData(left, right, ComparisonOperation.LessThan, isLessThan)
                .SetName(testNamePrefix + " - LessThan");
            yield return new TestCaseData(left, right, ComparisonOperation.GreaterThanOrEqual, isGreaterThan || areEqual)
                .SetName(testNamePrefix + " - GreaterThanOrEqual");
            yield return new TestCaseData(left, right, ComparisonOperation.LessThanOrEqual, isLessThan || areEqual)
                .SetName(testNamePrefix + " - LessThanOrEqual");

            //Comparison operations are commutative, so test the reverse direction
            yield return new TestCaseData(right, left, ComparisonOperation.Equal, areEqual)
                .SetName(testNamePrefix + " - Reverse Equal");
            yield return new TestCaseData(right, left, ComparisonOperation.NotEqual, !areEqual)
                .SetName(testNamePrefix + " - Reverse NotEqual");
            yield return new TestCaseData(right, left, ComparisonOperation.GreaterThan, isLessThan)
                .SetName(testNamePrefix + " - Reverse GreaterThan");
            yield return new TestCaseData(right, left, ComparisonOperation.LessThan, isGreaterThan)
                .SetName(testNamePrefix + " - Reverse LessThan");
            yield return new TestCaseData(right, left, ComparisonOperation.GreaterThanOrEqual, isLessThan || areEqual)
                .SetName(testNamePrefix + " - Reverse GreaterThanOrEqual");
            yield return new TestCaseData(right, left, ComparisonOperation.LessThanOrEqual, isGreaterThan || areEqual)
                .SetName(testNamePrefix + " - Reverse LessThanOrEqual");
        }


        private void ComparisonTest(object left, object right, ComparisonOperation operation, bool expected, string message = null)
        {
            var binder = new VelocityComparisonOperationBinder(operation);

            var result = Utility.BinderTests<bool>(binder, left, right);

            Assert.AreEqual(expected, result);
        }

        public enum TestEnum
        {
            Red = 1,
            Green = 2,
            Blue = 3
        }

        public class OverloadedEquals
        {
            public static bool operator ==(OverloadedEquals left, OverloadedEquals right)
            {
                return true;
            }

            public static bool operator !=(OverloadedEquals left, OverloadedEquals right)
            {
                return false;
            }
        }

        public class OverriddenEquals
        {
            public override int GetHashCode()
            {
                return 1;
            }

            public override bool Equals(object obj)
            {
                return obj is OverriddenEquals;
            }

        }

    }
}
