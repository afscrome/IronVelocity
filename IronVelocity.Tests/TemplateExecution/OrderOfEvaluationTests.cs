using Moq;
using Moq.Sequences;
using NUnit.Framework;
using System.Collections.Generic;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class OrderOfEvaluationTests : TemplateExeuctionBase
    {
        public OrderOfEvaluationTests(StaticTypingMode mode) : base(mode)
        {
        }

        [TestCase("+")]
        [TestCase("-")]
        [TestCase("*")]
        [TestCase("/")]
        [TestCase("%")]
        [TestCase(">")]
        [TestCase("<")]
        [TestCase(">=")]
        [TestCase("<=")]
        [TestCase("==")]
        [TestCase("!=")]
        //Can't test && or || here due to their short circuiting behavior
        public void When_EvaluatinngABinaryExpression_OperandsAreEvaluatedLeftToRight(string @operator)
        {
            var input = $"$helper.One {@operator} $helper.Two";

            var mock = new Mock<ITestHelper>();
            mock.Setup(x => x.One).Returns(123);
            mock.Setup(x => x.Two).Returns(456);

            using (Sequence.Create())
            {
                mock.Setup(x => x.One).InSequence(Times.Once());
                mock.Setup(x => x.Two).InSequence(Times.Once());
                var context = new Dictionary<string, object>
                {
                    ["helper"] = mock.Object
                };

                var result = EvaluateExpression(input, context);
            }

            mock.Verify(x => x.One, Times.Once);
            mock.Verify(x => x.Two, Times.Once);
        }

        [Test]
        public void When_EvaluatingAListExpression_OperandsAreEvaluatedLeftToRight()
        {
            var input = $"[$helper.One, $helper.Two, $helper.Three]";

            var mock = new Mock<ITestHelper>();
            mock.Setup(x => x.One).Returns(123);
            mock.Setup(x => x.Two).Returns("hello");
            mock.Setup(x => x.Two).Returns(true);

            using (Sequence.Create())
            {
                mock.Setup(x => x.One).InSequence(Times.Once());
                mock.Setup(x => x.Two).InSequence(Times.Once());
                mock.Setup(x => x.Three).InSequence(Times.Once());
                var context = new Dictionary<string, object>
                {
                    ["helper"] = mock.Object
                };

                EvaluateExpression(input, context);
            }

            mock.Verify(x => x.One, Times.Once);
            mock.Verify(x => x.Two, Times.Once);
            mock.Verify(x => x.Three, Times.Once);
        }


        [Test]
        public void When_EvaluatingMethodArguments_OperandsAreEvaluatedLeftToRight()
        {
            var input = $"$foo.Method($helper.One, $helper.Two, $helper.Three)";

            var mock = new Mock<ITestHelper>();
            mock.Setup(x => x.One).Returns(123);
            mock.Setup(x => x.Two).Returns("hello");
            mock.Setup(x => x.Two).Returns(true);

            using (Sequence.Create())
            {
                mock.Setup(x => x.One).InSequence(Times.Once());
                mock.Setup(x => x.Two).InSequence(Times.Once());
                mock.Setup(x => x.Three).InSequence(Times.Once());
                var context = new Dictionary<string, object>
                {
                    ["helper"] = mock.Object,
                    ["foo"] = new object()
                };

                EvaluateExpression(input, context);
            }

            mock.Verify(x => x.One, Times.Once);
            mock.Verify(x => x.Two, Times.Once);
            mock.Verify(x => x.Three, Times.Once);
        }

        [Test]
        [Ignore("TODO: come back and fix")]
        public void When_EvaluatingMethodArgumentsOnNullReference_OperandsAreNotEvaluated()
        {
            var input = $"$foo.Method($helper.One)";

            var mock = new Mock<ITestHelper>();
            mock.Setup(x => x.One).Returns(() =>
            {
                return 123;
            });

            var context = new Dictionary<string, object>
            {
                ["helper"] = mock.Object
            };

            EvaluateExpression(input, context);

            mock.Verify(x => x.One, Times.Never);
        }

        [Test]
        public void When_EvaluatinngAnAndExpression_RightOperandShortCircuitedIfLeftIsFalse()
        {
            var mock = new Mock<ITestHelper>();
            mock.Setup(x => x.One).Returns(false);
            mock.Setup(x => x.Two).Returns(true);

            var context = new Dictionary<string, object>
            {
                ["helper"] = mock.Object
            };

            EvaluateExpression("$helper.One && $helper.Two", context);

            mock.Verify(x => x.One, Times.Once);
            mock.Verify(x => x.Two, Times.Never);
        }

        [Test]
        public void When_EvaluatinngAnOrExpression_RightOperandShortCircuitedIfLeftIsTrue()
        {
            var mock = new Mock<ITestHelper>();
            mock.Setup(x => x.One).Returns(true);
            mock.Setup(x => x.Two).Returns(false);

            var context = new Dictionary<string, object>
            {
                ["helper"] = mock.Object
            };

            EvaluateExpression("$helper.One || $helper.Two", context);

            mock.Verify(x => x.One, Times.Once);
            mock.Verify(x => x.Two, Times.Never);
        }

        public interface ITestHelper
        {
            object One { get; }
            object Two { get; }
            object Three { get; }
        }
    }
}
