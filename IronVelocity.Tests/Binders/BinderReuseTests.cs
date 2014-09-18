using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests.Binders
{
    /// <summary>
    /// By ensuring we reuse CallSiteBinders, we can make use of the L2 CallSite cache, which reduces the amount of compilation & JIT we have to do.
    /// </summary>
    [TestFixture]
    public class BinderReuseTests
    {
        private BinderHelper _oldHelper;
        [TestFixtureSetUp]
        public void SetUp()
        {
            _oldHelper = BinderHelper.Instance;
            BinderHelper.Instance = new DuplicateBindeHelper();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            BinderHelper.Instance = _oldHelper;
        }



        [Test]
        public void InvokeMemberReuse()
        {
            var context = new Dictionary<string, object>{
                {"x", 123}
            };

            var input = "$x.ToString() $x.ToString()";
            var expected = "123 123";

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseDifferenceSignaturesWithConstants()
        {
            var context = new Dictionary<string, object>{
                {"x", new InvokeMemberHelper()}
            };

            var input = "$x.Double(123) $x.Double('hello')";
            var expected = "246 hellohello";

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseDifferenceSignaturesWithVariables()
        {
            var context = new Dictionary<string, object>{
                {"x", new InvokeMemberHelper()},
                {"a", "hello"},
                {"b", 123}
            };

            var input = "$x.Double($a) $x.Double($b)";
            var expected = "hellohello 246";

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseCompatibleSignatures1()
        {
            var context = new Dictionary<string, object>{
                {"x", new InvokeMemberHelper()},
                {"y", 1.3}
            };

            var input = "$x.Double($y) $x.Double(123)";
            var expected = "2.6 246";

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);
        }

        [Test]
        public void InvokeMemberReuseDoesNotConfuseCompatibleSignatures2()
        {
            var context = new Dictionary<string, object>{
                {"x", new InvokeMemberHelper()},
                {"y", 1.3}
            };

            var input = "$x.Double(123) $x.Double($y)";
            var expected = "246 2.6";

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);
        }



        [Test]
        public void GetMemberReuse()
        {
            var context = new Dictionary<string, object>{
                {"x", "testing 1 2 3"}
            };

            var input = "$x.Length $x.Length";
            var expected = "13 13";

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);
        }

        [Test]
        public void SetMemberReuse()
        {
            throw new NotImplementedException();
            var context = new Dictionary<string, object>{
                {"x", "testing 1 2 3"}
            };

            var input = "$x.Length $x.Length";
            var expected = "13 13";

            Utility.TestExpectedMarkupGenerated(input, expected, context, isGlobalEnvironment: false);
        }

        public class InvokeMemberHelper
        {
            public double Double(double value)
            {
                return value * 2;
            }

            public string Double(string value)
            {
                return value.ToString() + value.ToString();
            }
        }

        public class DuplicateBindeHelper : BinderHelper{

            protected override GetMemberBinder CreateGetMemberBinder(string memberName)
            {
                return new DupDetectionGetMemberBinder(memberName);
            }
            protected override SetMemberBinder CreateSetMemberBinder(string memberName)
            {
                return new DupDetectionSetMemberBinder(memberName);
            }

            public class DupDetectionGetMemberBinder : VelocityGetMemberBinder
            {
                public DupDetectionGetMemberBinder(string name)
                    : base(name)
                {
                }

                public static int CallCount { get; private set; }

                public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                {
                    if (CallCount > 0)
                        Assert.Fail("FallbackGetMember was called twice");

                    CallCount++;

                    return base.FallbackGetMember(target, errorSuggestion);
                }
            }
            public class DupDetectionSetMemberBinder : VelocitySetMemberBinder
            {
                public DupDetectionSetMemberBinder(string name)
                    : base(name)
                {
                }

                public static int CallCount { get; private set; }

                public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
                {
                    if (CallCount > 0)
                        Assert.Fail();

                    CallCount++;

                    return base.FallbackSetMember(target, errorSuggestion);
                }
            }

            public class DupDetectionInvokeMemberBinder : VelocityInvokeMemberBinder
            {
                public DupDetectionInvokeMemberBinder(string name, CallInfo callInfo)
                    : base(name, callInfo)
                {
                }

                public static int CallCount { get; private set; }

                public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
                {
                    if (CallCount > 0)
                        Assert.Fail("FallbackInvokeMember was called twice");

                    CallCount++;

                    return base.FallbackInvokeMember(target, args, errorSuggestion);
                }
            }

        }



    }
}
