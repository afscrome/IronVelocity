using IronVelocity.RuntimeHelpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests
{
    [TestFixture]
    public class IntegerRangeTests
    {
        [TestCase(0, 0, new[] { 0 }, TestName = "Identity Range")]
        [TestCase(4, 7, new[] { 4, 5, 6, 7 }, TestName = "Ascending Range")]
        [TestCase(9, 5, new[] { 9, 8, 7, 6, 5 }, TestName = "Descending Range")]
        [TestCase(-14, -11, new[] { -14,-13,-12,-11 }, TestName = "Negative Ascending Range")]
        [TestCase(-3, -5, new[] { -3,-4,-5 }, TestName = "Negative Descending Range")]
        [TestCase(-2, 2, new[] { -2, -1, 0, 1, 2 }, TestName = "Ascending Range across 0")]
        [TestCase(2, -2, new[] { 2, 1, 0, -1, -2 }, TestName = "Negative Range across 0")]
        [TestCase(null, 5, null, TestName = "Null left")]
        [TestCase(23, null, null, TestName = "Null right")]
        [TestCase(null, null, null, TestName = "Null both")]
        public void Test(object left, object right, int[] expected)
        {
            var result = IntegerRange.Range(left, right);
            CollectionAssert.AreEqual(expected, result);
        }
    }
}
