using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Tests.Binders
{
    [TestFixture]
    public class ComparisonBinderTests : BinderTestBase
    {
        [TestCaseSource("TestCases")]
        public void Test(object left, object right, ExpressionType operation, bool? expected)
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
                GenerateTestCaseData(1, TestEnum.Red, false, null, null, "Equal Integer Enum"),
                GenerateTestCaseData(TestEnum.Green, 2, false, null, null, "Equal Enum Integer"),
                GenerateTestCaseData(TestEnum.Blue, "Blue", true, null, null, "Equal Enum Identical Case String"),
                GenerateTestCaseData(TestEnum.Blue, "BLUE", false, null, null, "Equal Enum Differing Case String"),
                GenerateTestCaseData(TestEnum.Green, 1, false, null, null, "GreaterThan Enum"),
                GenerateTestCaseData(7, 7L, true, false, false, "NotEqual Enum String"),
                GenerateTestCaseData(true, true, true, null, null, "True True"),
                GenerateTestCaseData(true, false, false, null, null, "True False"),
                GenerateTestCaseData(false, true, false, null, null, "False True"),
                GenerateTestCaseData(false, false, true, null, null, "False False"),
                GenerateTestCaseData(null, null, true, null, null, "null null"),
                GenerateTestCaseData("foo", null, false, null, null, "object null"),
                GenerateTestCaseData(null, "bar", false, null, null, "null object"),
                GenerateTestCaseData("Hello World!", "Hello World!", true, null, null, "Identical Case Strings"),
                GenerateTestCaseData("HELLO WORLD!", "hello world!", false, null, null, "Differing Case Strings"),
                GenerateTestCaseData(DateTime.Now.AddMinutes(-1), DateTime.Now, false, false, true, "DateTime"),
                GenerateTestCaseData(objectReference, objectReference, true, null, null, "Object Identical References"),
                GenerateTestCaseData(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(50), false, true, false, "TimeSpan"),
                GenerateTestCaseData(guid1, guid2, true, null, null, "Structs with same values but different instances"),
                GenerateTestCaseData(new OverloadedEquals(), new OverloadedEquals(), true, null, null, "Object with Overloaded Equality Operators"),
            }.SelectMany(x => x);
        }


        private IEnumerable<TestCaseData> GenerateTestCaseData(object left, object right, bool areEqual, bool? isGreaterThan, bool? isLessThan, string testNamePrefix)
        {
            if ((isGreaterThan ?? false) && (isLessThan ?? false))
                throw new InvalidOperationException("Two objects cannot be both greater & less than each other - " + testNamePrefix);
            if (areEqual && (isGreaterThan ?? false))
                throw new InvalidOperationException("Two equal objects cannot be greater than each other - " + testNamePrefix);
            if (areEqual && (isLessThan ?? false))
                throw new InvalidOperationException("Two equal objects cannot be less than each other - " + testNamePrefix);

            testNamePrefix = "ComparisonBinderTest - " + testNamePrefix;

            yield return new TestCaseData(left, right, ExpressionType.Equal, areEqual)
                .SetName(testNamePrefix + " - Equal");
            yield return new TestCaseData(left, right, ExpressionType.NotEqual, !areEqual)
                .SetName(testNamePrefix + " - NotEqual");
            yield return new TestCaseData(left, right, ExpressionType.GreaterThan, isGreaterThan)
                .SetName(testNamePrefix + " - GreaterThan");
            yield return new TestCaseData(left, right, ExpressionType.LessThan, isLessThan)
                .SetName(testNamePrefix + " - LessThan");
            yield return new TestCaseData(left, right, ExpressionType.GreaterThanOrEqual, (isGreaterThan ?? false) || areEqual)
                .SetName(testNamePrefix + " - GreaterThanOrEqual");
            yield return new TestCaseData(left, right, ExpressionType.LessThanOrEqual, (isLessThan ?? false) || areEqual)
                .SetName(testNamePrefix + " - LessThanOrEqual");

            //Comparison operations are commutative, so test the reverse direction
            yield return new TestCaseData(right, left, ExpressionType.Equal, areEqual)
                .SetName(testNamePrefix + " - Reverse Equal");
            yield return new TestCaseData(right, left, ExpressionType.NotEqual, !areEqual)
                .SetName(testNamePrefix + " - Reverse NotEqual");
            yield return new TestCaseData(right, left, ExpressionType.GreaterThan, isLessThan)
                .SetName(testNamePrefix + " - Reverse GreaterThan");
            yield return new TestCaseData(right, left, ExpressionType.LessThan, isGreaterThan)
                .SetName(testNamePrefix + " - Reverse LessThan");
            yield return new TestCaseData(right, left, ExpressionType.GreaterThanOrEqual, (isLessThan ?? false) || areEqual)
                .SetName(testNamePrefix + " - Reverse GreaterThanOrEqual");
            yield return new TestCaseData(right, left, ExpressionType.LessThanOrEqual, (isGreaterThan ?? false) || areEqual)
                .SetName(testNamePrefix + " - Reverse LessThanOrEqual");
        }


        private void ComparisonTest(object left, object right, ExpressionType operation, bool? expected, string message = null)
        {
            var binder = new VelocityBinaryOperationBinder(operation);

            var result = InvokeBinder(binder, left, right);

            Assert.AreEqual(expected, result);
        }

        public enum TestEnum
        {
            Red = 1,
            Green = 2,
            Blue = 3
        }

#pragma warning disable CS0660,CS0661
        public class OverloadedEquals
        {
            public static bool operator ==(OverloadedEquals left, OverloadedEquals right) => true;
            public static bool operator !=(OverloadedEquals left, OverloadedEquals right) => false;
        }
#pragma warning restore CS0660,CS0661

        public class OverriddenEquals
        {
            public override int GetHashCode() => 1;
            public override bool Equals(object obj) => obj is OverriddenEquals;
        }

    }
}
