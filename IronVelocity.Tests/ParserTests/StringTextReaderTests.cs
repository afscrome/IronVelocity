using IronVelocity.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.ParserTests
{
    public class StringTextReaderTests
    {
        public StringTextWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = new StringTextWindow("Hello world 123");
        }

        [Test]
        public void StartPositionIsCorrect()
        {
            Assert.That(_window.StartPosition, Is.EqualTo(0));
        }

        [Test]
        public void EndPositionIsCorrect()
        {
            Assert.That(_window.EndPosition, Is.EqualTo(15));
        }

        [Test]
        public void ReturnsEndOfFileAtEndOfFile()
        {
            var window = new StringTextWindow("");

            Assert.That(window.CurrentChar, Is.EqualTo('\0'));
        }

        [Test]
        public void ThrowsWhenMovingPastEndOfFile()
        {
            var window = new StringTextWindow("");

            Assert.That(() => window.MoveNext(), Throws.InstanceOf<InvalidOperationException>());
        }


        [TestCase(0, 0, "H")]
        [TestCase(4, 9, "o worl")]
        [TestCase(14, 14, "3")]
        public void GetRangeTests(int start, int end, string expected)
        {
            Assert.That(_window.GetRange(start, end), Is.EqualTo(expected));
        }

    }
}
